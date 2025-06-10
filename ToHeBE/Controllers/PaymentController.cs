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
		var returnUrl = _config["VnPay:ReturnUrl"];        // http://localhost:3000/paymentReuslt
		var tmnCode = _config["VnPay:TmnCode"];          // FLQYP5IJ
		var hashSecret = _config["VnPay:HashSecret"];       // JBOUUFLRZBNYQBEQHKFOHSCDRSVTNVRM

		// ====== B3. Thông tin giao dịch ======
		var txnRef = new Random().Next(10000000, 99999999).ToString();
		var orderInfo = "Thanh toan VNPAY";
		var amount = (10000000).ToString(CultureInfo.InvariantCulture);
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

	/*[HttpGet("vnpay-return")]
	public IActionResult VnPayReturn()
	{
		var query = HttpContext.Request.Query;

		var vnp_ResponseCode = query["vnp_ResponseCode"];
		var vnp_TxnRef = query["vnp_TxnRef"];
		var vnp_Amount = query["vnp_Amount"];
		var vnp_SecureHash = query["vnp_SecureHash"];

		// TODO: Xác minh lại chữ ký vnp_SecureHash để đảm bảo dữ liệu không bị giả mạo

		if (vnp_ResponseCode == "00")
		{
			// Thanh toán thành công
			return Redirect("http://localhost:3000/PaymentResult"); // hoặc return View()
		}
		else
		{
			// Thanh toán thất bại
			return Redirect("http://localhost:3000/PaymentResult");
		}
	}*/

}

public class PaymentRequest
{
	public double Amount { get; set; }
}