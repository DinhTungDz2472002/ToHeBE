using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
	private readonly IConfiguration _configuration;

	public PaymentController(IConfiguration configuration)
	{
		_configuration = configuration;
	}

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
		foreach (var key in query.Keys)
		{
			if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
			{
				vnpay.AddRequestData(key, WebUtility.UrlDecode(query[key]));
			}
		}

		string vnp_SecureHash = query["vnp_SecureHash"];
		string hashSecret = _configuration.GetSection("Vnpay")["HashSecret"];
		string signData = string.Join("&", vnpay.GetRequestData().OrderBy(k => k.Key).Select(k => $"{k.Key}={WebUtility.UrlEncode(k.Value)}"));
		string computedHash = vnpay.HmacSHA512(hashSecret, signData);

		// Log để kiểm tra
		Console.WriteLine($"signData: {signData}");
		Console.WriteLine($"computedHash: {computedHash}");
		Console.WriteLine($"vnp_SecureHash: {vnp_SecureHash}");
		Console.WriteLine($"vnp_ResponseCode: {query["vnp_ResponseCode"]}");

		if (computedHash == vnp_SecureHash && query["vnp_ResponseCode"] == "00")
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
				Message = $"Thanh toán thất bại. ResponseCode: {query["vnp_ResponseCode"]}, SignatureValid: {computedHash == vnp_SecureHash}"
			});
		}
	}

	private string GetClientIpAddress()
	{
		return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
	}
}

public class PaymentRequest
{
	public double Amount { get; set; }
}