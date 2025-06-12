using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using System.Security.Claims;
using ToHeBE.Models.Auth;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VnPayController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly ToHeDbContext dbContext;
	
		public VnPayController(IConfiguration configuration, ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
			_config = configuration;
		}

		[HttpPost("create-payment")]
		public async Task<IActionResult> CreatePayment([FromBody] VnpayRequest request)
		{
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { code = "401", message = "Không tìm thấy thông tin người dùng" });
			}

			// Lấy thông tin khách hàng
			var khachHang = await dbContext.Tkhachhangs.FindAsync(int.Parse(userId));
			if (khachHang == null)
			{
				return NotFound(new { code = "404", message = "Khách hàng không tồn tại" });
			}
			// Tìm hóa đơn
			Thdb order;
			if (request.maHdb.HasValue)
			{
				// Nếu có maHdb trong request, lấy hóa đơn theo maHdb
				order = await dbContext.Thdbs
					.Where(h => h.MaHdb == request.maHdb && h.MaKhachHang == int.Parse(userId) && h.Pttt == "Chờ thanh toán")
					.FirstOrDefaultAsync();
			}
			else
			{
				// Nếu không có maHdb, lấy hóa đơn mới nhất theo thời gian lập hóa đơn
				order = await dbContext.Thdbs
					.Where(h => h.MaKhachHang == int.Parse(userId) && h.Pttt == "Chờ thanh toán")
					.OrderByDescending(h => h.NgayLapHdb)
					.FirstOrDefaultAsync();
			}
			if (order == null)
			{
				return BadRequest(new { code = "400", message = "Không tìm thấy hóa đơn chờ thanh toán hoặc hóa đơn không hợp lệ!" });
			}

			// VnPay configuration
			var vnpUrl = _config["VnPay:BaseUrl"];
			var returnUrl = _config["VnPay:ReturnUrl"];
			var tmnCode = _config["VnPay:TmnCode"];
			var hashSecret = _config["VnPay:HashSecret"];

			if (string.IsNullOrEmpty(vnpUrl) || string.IsNullOrEmpty(returnUrl) ||
				string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
			{
				return StatusCode(500, new { code = "500", message = "Cấu hình VnPay không hợp lệ!" });
			}

			//var txnRef = order.MaHdb.ToString();
			var txnRef = new Random().Next(10000000, 99999999).ToString();
			//var orderInfo = $"Thanh toan hoa don {order.MaHdb}";
			var orderInfo = $" {order.MaHdb}";
			var amount = ((int)(order.TongTienHdb * 100)).ToString(CultureInfo.InvariantCulture); // Use TongTienHdb from order
			var locale = "vn";
			var bankCode = "";

			var inputData = new SortedDictionary<string, string>
			{
				["vnp_Version"] = "2.1.0",
				["vnp_Command"] = "pay",
				["vnp_TmnCode"] = tmnCode,
				["vnp_Amount"] = amount,
				["vnp_CurrCode"] = "VND",
				["vnp_TxnRef"] = txnRef,
				["vnp_OrderInfo"] = orderInfo,
				["vnp_OrderType"] = "billpayment",
				["vnp_ReturnUrl"] = returnUrl,
				["vnp_IpAddr"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
				["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
				["vnp_Locale"] = locale
			};
			if (!string.IsNullOrWhiteSpace(bankCode))
				inputData["vnp_BankCode"] = bankCode;

			var (hashData, queryString) = BuildQueryAndHash(inputData);
			var secureHash = ComputeHash(hashSecret, hashData);
			var payUrl = $"{vnpUrl}?{queryString}vnp_SecureHash={secureHash}";

			return Ok(new
			{
				code = "00",
				message = "success",
				data = payUrl
			});
		}


		[HttpGet("vnpay-return")]
		public async Task<IActionResult> VnPayReturn()
		{
			Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] VnPayReturn endpoint called");
			Console.WriteLine($"Query params: {HttpContext.Request.QueryString}");

			var query = HttpContext.Request.Query;
			var vnp_ResponseCode = query["vnp_ResponseCode"];
			var vnp_TxnRef = query["vnp_TxnRef"];
			var vnp_SecureHash = query["vnp_SecureHash"];
			var vnp_Amount = query["vnp_Amount"];
			var vnp_OrderInfo = query["vnp_OrderInfo"];

			// Verify secure hash
			var inputData = new SortedDictionary<string, string>();
			foreach (var key in query.Keys.Where(k => k != "vnp_SecureHash"))
			{
				inputData[key] = query[key];
			}
			var (hashData, _) = BuildQueryAndHash(inputData);
			var computedHash = ComputeHash(_config["VnPay:HashSecret"], hashData);


			if (!int.TryParse(vnp_OrderInfo, out var orderId))
			{
				Console.WriteLine($"[{DateTime.UtcNow}] Invalid order ID: {vnp_TxnRef}");
				return BadRequest(new { code = "400", message = "Mã hóa đơn không hợp lệ!" });
			}

			var order = await dbContext.Thdbs.FindAsync(orderId);
			if (order == null)
			{
				Console.WriteLine($"[{DateTime.UtcNow}] Order not found: {orderId}");
				return NotFound(new { code = "404", message = "Hóa đơn không tồn tại!" });
			}

			if (vnp_ResponseCode == "00")
			{
				// Payment success
				order.Pttt = "Đã thanh toán qua VnPay";
				order.Status = "Chờ xác nhận";
				await dbContext.SaveChangesAsync();
				Console.WriteLine($"[{DateTime.UtcNow}] Order {orderId} updated to success: PTTT=Đã thanh toán qua VnPay, Status=Đã xác nhận");
			}
			else
			{
				// Payment failed or canceled
				order.Pttt = "Chờ thanh toán";
				await dbContext.SaveChangesAsync();
				Console.WriteLine($"[{DateTime.UtcNow}] Order {orderId} updated to pending: PTTT=Chờ thanh toán");
			}

			// Redirect to frontend payment result page
			var redirectUrl = $"http://localhost:3000/PaymentResult{HttpContext.Request.QueryString}";
			Console.WriteLine($"[{DateTime.UtcNow}] Redirecting to: {redirectUrl}");
			return Redirect(redirectUrl);
		}

		private static (string hashData, string queryString) BuildQueryAndHash(SortedDictionary<string, string> input)
		{
			var sbHash = new StringBuilder();
			var sbQuery = new StringBuilder();
			foreach (var kv in input)
			{
				sbHash.Append(sbHash.Length > 0 ? '&' : char.MinValue)
					  .Append(WebUtility.UrlEncode(kv.Key))
					  .Append('=')
					  .Append(WebUtility.UrlEncode(kv.Value));

				sbQuery.Append(WebUtility.UrlEncode(kv.Key))
					   .Append('=')
					   .Append(WebUtility.UrlEncode(kv.Value))
					   .Append('&');
			}
			var hashData = sbHash.ToString().TrimStart('\0');
			var queryString = sbQuery.ToString();
			return (hashData, queryString);
		}

		private static string ComputeHash(string secret, string data)
		{
			using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
			var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
			return string.Concat(hashBytes.Select(b => b.ToString("x2")));
		}
	}

	
}