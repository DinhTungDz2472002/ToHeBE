using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ToHeBE.Models;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
	private readonly IConfiguration _config;
	private readonly ToHeDbContext dbContext;


	public PaymentController(IConfiguration configuration, ToHeDbContext dbContext
		)
	{
		this.dbContext = dbContext;
		_config = configuration;
	}


	[HttpPost("create-payment")]
	public IActionResult VnPay()
	{
		// ====== B1. Lấy thông tin giỏ hàng ======

		/*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "MaKhachHang");
		if (userIdClaim == null) return Unauthorized("Không tìm thấy mã khách hàng trong token!");

		var userId = int.Parse(userIdClaim.Value);
		var cart = dbContext.Tgiohangs.FirstOrDefault(c => c.MaKhachHang == userId);
		if (cart == null) return BadRequest("Giỏ hàng không tồn tại!");*/

		// ====== B2. Các hằng số cấu hình ======
		var vnpUrl = _config["VnPay:BaseUrl"];          // https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
		var returnUrl = _config["VnPay:ReturnUrl"];        // http://localhost:4200/bill
		var tmnCode = _config["VnPay:TmnCode"];          // FLQYP5IJ
		var hashSecret = _config["VnPay:HashSecret"];       // JBOUUFLRZBNYQBEQHKFOHSCDRSVTNVRM

		// ====== B3. Thông tin giao dịch ======
		var txnRef = new Random().Next(10000000, 99999999).ToString();
		var orderInfo = "Thanh toan VNPAY";
		var amount = (1000000).ToString(CultureInfo.InvariantCulture);
		var locale = "vn";
		var bankCode = "NCB";

		// ====== B4. Build inputData Dictionary (KSORT) ======
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

		// ====== B5. Tạo chuỗi hashData & queryString ======
		var (hashData, queryString) = BuildQueryAndHash(inputData);

		// ====== B6. Tính vnp_SecureHash (HMAC SHA512) ======
		var secureHash = ComputeHash(hashSecret, hashData);

		var payUrl = $"{vnpUrl}?{queryString}vnp_SecureHash={secureHash}";

		// ====== B7. Trả kết quả JSON ======
		return Ok(new
		{
			code = "00",
			message = "success",
			data = payUrl
		});

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
		var hashData = sbHash.ToString().TrimStart('\0'); // bỏ ký tự đầu 0
		var queryString = sbQuery.ToString();
		return (hashData, queryString);
	}
	private static string ComputeHash(string secret, string data)
	{
		using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
		var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
		return string.Concat(hashBytes.Select(b => b.ToString("x2")));
	}


	/*
		[HttpPost("create-payment")]
		public IActionResult CreatePayment([FromBody] PaymentRequest request)
		{
			try
			{
				var vnpay = new VnPayLibrary();
				var vnpayConfig = _configuration.GetSection("Vnpay");

				vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
				vnpay.AddRequestData("vnp_Command", "pay");
				vnpay.AddRequestData("vnp_TmnCode", vnpayConfig["TmnCode"]);
				vnpay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
				vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
				vnpay.AddRequestData("vnp_CurrCode", "VND");
				vnpay.AddRequestData("vnp_IpAddr", GetClientIpAddress());
				vnpay.AddRequestData("vnp_Locale", "vn");
				vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {DateTime.Now.Ticks}");
				vnpay.AddRequestData("vnp_OrderType", "other");
				vnpay.AddRequestData("vnp_ReturnUrl", vnpayConfig["ReturnUrl"]);
				vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

				string paymentUrl = vnpay.CreateRequestUrl(vnpayConfig["BaseUrl"], vnpayConfig["HashSecret"]);
				return Ok(new { PaymentUrl = paymentUrl });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = "Lỗi tạo URL thanh toán", Error = ex.Message });
			}
		}

		[HttpGet("vnpay-return")]
		public IActionResult VnPayReturn()
		{
			var query = Request.Query;
			var vnpay = new VnPayLibrary();

			// Lấy tất cả tham số bắt đầu bằng "vnp_", trừ vnp_SecureHash và vnp_SecureHashType
			foreach (var key in query.Keys)
			{
				if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
				{
					vnpay.AddRequestData(key, query[key]); // Lưu giá trị thô
					Console.WriteLine($"Tham số: {key}={query[key]}"); // Ghi log để kiểm tra
				}
			}

			string vnp_SecureHash = query["vnp_SecureHash"];
			if (string.IsNullOrEmpty(vnp_SecureHash))
			{
				return BadRequest(new { Result = "Failed", Message = "Thiếu vnp_SecureHash" });
			}

			string hashSecret = _configuration.GetSection("Vnpay")["HashSecret"];
			// Tạo chuỗi ký từ các tham số, không mã hóa lại
			string signData = string.Join("&", vnpay.GetRequestData().OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}"));
			string computedHash = vnpay.HmacSHA512(hashSecret, signData);

			// Ghi log để kiểm tra
			Console.WriteLine($"Chuỗi ký: {signData}");
			Console.WriteLine($"Chữ ký tính được: {computedHash}");
			Console.WriteLine($"Chữ ký từ VNPay: {vnp_SecureHash}");
			Console.WriteLine($"Mã phản hồi: {query["vnp_ResponseCode"]}");

			// So sánh chữ ký (không phân biệt hoa thường) và kiểm tra mã phản hồi
			if (computedHash.Equals(vnp_SecureHash, StringComparison.OrdinalIgnoreCase) && query["vnp_ResponseCode"] == "00")
			{
				var amount = int.Parse(query["vnp_Amount"]) / 100;
				var orderId = query["vnp_TxnRef"];
				var vnpayTranId = query["vnp_TransactionNo"];
				return Ok(new
				{
					Result = "Success",
					OrderId = orderId,
					Amount = amount,
					VnpayTranId = vnpayTranId,
					ResponseCode = query["vnp_ResponseCode"]
				});
			}
			else
			{
				return BadRequest(new
				{
					Result = "Failed",
					Message = $"Thanh toán thất bại. Mã phản hồi: {query["vnp_ResponseCode"]}, Chữ ký hợp lệ: {computedHash == vnp_SecureHash}"
				});
			}
		}
		private string GetClientIpAddress()
		{
			return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
		}*/
}

public class PaymentRequest
{
	public double Amount { get; set; }
}