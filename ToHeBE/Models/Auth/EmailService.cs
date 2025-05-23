using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Web;

namespace ToHeBE.Models.Auth
{
	public class EmailService
	{
		private readonly IConfiguration _configuration;

		public EmailService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		/*cấu hình lấy lại mật khẩu*/
		public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
		{
			var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
			{
				Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
				Credentials = new NetworkCredential(
					_configuration["EmailSettings:Username"],
					_configuration["EmailSettings:Password"]
				),
				EnableSsl = true
			};

			var resetLink = $"http://localhost:3000/reset-password?token={resetToken}";
			var mailMessage = new MailMessage
			{
				From = new MailAddress(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderName"]),
				Subject = "Khôi Phục Mật Khẩu",
				Body = $"<h2>Khôi phục mật khẩu</h2>" +
					   $"<p>Nhấp vào liên kết sau để đặt lại mật khẩu:</p>" +
					   $"<a href='{resetLink}'>Đặt lại mật khẩu</a>" +
					   "<p>Liên kết sẽ hết hạn sau 1 giờ.</p>" +
					   "<p>Trân trọng,<br>Your App Name</p>",
				IsBodyHtml = true
			};
			mailMessage.To.Add(toEmail);

			await smtpClient.SendMailAsync(mailMessage);
		}
		/*cấu hình gửi hóa đơn vào email*/
		public async Task SendOrderConfirmationEmailAsync(string toEmail, Thdb hoaDon, List<Tchitiethdb> chiTietHdb, string tenKhachHang)
		{
			var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
			{
				Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
				Credentials = new NetworkCredential(
					_configuration["EmailSettings:Username"],
					_configuration["EmailSettings:Password"]
				),
				EnableSsl = true
			};

			var bodyBuilder = new StringBuilder();
			bodyBuilder.AppendLine("<!DOCTYPE html>");
			bodyBuilder.AppendLine("<html lang='vi'>");
			bodyBuilder.AppendLine("<head>");
			bodyBuilder.AppendLine("<meta charset='UTF-8'>");
			bodyBuilder.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
			bodyBuilder.AppendLine("<title>Xác Nhận Đơn Hàng</title>");
			bodyBuilder.AppendLine("</head>");
			bodyBuilder.AppendLine("<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>");

			// Header với logo
			bodyBuilder.AppendLine("<div style='text-align: center; margin-bottom: 20px;'>");
			bodyBuilder.AppendLine("<img src='http://localhost:3000/assets/images/logo.png' alt='Your App Name' style='max-width: 150px; height: auto;'>");
			bodyBuilder.AppendLine("<h2 style='color: #1a73e8;'>Xác Nhận Đơn Hàng</h2>");
			bodyBuilder.AppendLine("</div>");

			// Thông tin đơn hàng
			bodyBuilder.AppendLine("<div style='background-color: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
			bodyBuilder.AppendLine($"<p style='margin: 0;'>Kính gửi <strong>{HttpUtility.HtmlEncode(tenKhachHang)}</strong>,</p>");
			bodyBuilder.AppendLine("<p style='margin: 10px 0;'>Cảm ơn bạn đã đặt hàng tại Tò He - Nét Văn hóa Việt! Dưới đây là chi tiết đơn hàng của bạn:</p>");
			bodyBuilder.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 10px 0;'>");
			bodyBuilder.AppendLine($"<tr><td style='padding: 5px; font-weight: bold;'>Mã đơn hàng:</td><td style='padding: 5px;'>{hoaDon.MaHdb}</td></tr>");
			bodyBuilder.AppendLine($"<tr><td style='padding: 5px; font-weight: bold;'>Ngày đặt hàng:</td><td style='padding: 5px;'>{hoaDon.NgayLapHdb:dd/MM/yyyy HH:mm}</td></tr>");
			bodyBuilder.AppendLine($"<tr><td style='padding: 5px; font-weight: bold;'>Địa chỉ:</td><td style='padding: 5px;'>{HttpUtility.HtmlEncode(hoaDon.DiaChi)}</td></tr>");
			bodyBuilder.AppendLine($"<tr><td style='padding: 5px; font-weight: bold;'>Số điện thoại:</td><td style='padding: 5px;'>{HttpUtility.HtmlEncode(hoaDon.Sdt)}</td></tr>");
			bodyBuilder.AppendLine($"<tr><td style='padding: 5px; font-weight: bold;'>Trạng thái:</td><td style='padding: 5px;'>{HttpUtility.HtmlEncode(hoaDon.Status)}</td></tr>");
			bodyBuilder.AppendLine("</table>");
			bodyBuilder.AppendLine("</div>");

			// Chi tiết đơn hàng
			bodyBuilder.AppendLine("<h3 style='color: #1a73e8; margin-bottom: 10px;'>Chi Tiết Đơn Hàng</h3>");
			bodyBuilder.AppendLine("<table style='width: 100%; border-collapse: collapse; background-color: #fff; border: 1px solid #e0e0e0;'>");
			bodyBuilder.AppendLine("<thead>");
			bodyBuilder.AppendLine("<tr style='background-color: #1a73e8; color: #fff;'>");
			bodyBuilder.AppendLine("<th style='padding: 12px; text-align: left; border: 1px solid #e0e0e0;'>Sản phẩm</th>");
			bodyBuilder.AppendLine("<th style='padding: 12px; text-align: center; border: 1px solid #e0e0e0;'>Hình ảnh</th>");
			bodyBuilder.AppendLine("<th style='padding: 12px; text-align: center; border: 1px solid #e0e0e0;'>Số lượng</th>");
			bodyBuilder.AppendLine("<th style='padding: 12px; text-align: right; border: 1px solid #e0e0e0;'>Thành tiền</th>");
			bodyBuilder.AppendLine("</tr>");
			bodyBuilder.AppendLine("</thead>");
			bodyBuilder.AppendLine("<tbody>");

			foreach (var chiTiet in chiTietHdb)
			{
				var sanPham = chiTiet.MaSanPhamNavigation;
				var imageUrl = string.IsNullOrEmpty(sanPham.AnhSp)
					? "http://localhost:3000/assets/images/placeholder.jpg"
					: $"http://localhost:3000//assets/images/{HttpUtility.HtmlEncode(sanPham.AnhSp)}";
				bodyBuilder.AppendLine("<tr>");
				bodyBuilder.AppendLine($"<td style='padding: 12px; border: 1px solid #e0e0e0;'>{HttpUtility.HtmlEncode(sanPham.TenSanPham)}</td>");
				bodyBuilder.AppendLine($"<td style='padding: 12px; border: 1px solid #e0e0e0; text-align: center;'><img src='{imageUrl}' alt='{HttpUtility.HtmlEncode(sanPham.TenSanPham)}' style='max-width: 50px; height: auto; border-radius: 4px;'></td>");
				bodyBuilder.AppendLine($"<td style='padding: 12px; border: 1px solid #e0e0e0; text-align: center;'>{chiTiet.Sl}</td>");
				bodyBuilder.AppendLine($"<td style='padding: 12px; border: 1px solid #e0e0e0; text-align: right;'>{chiTiet.ThanhTien} VNĐ</td>");
				bodyBuilder.AppendLine("</tr>");
			}

			bodyBuilder.AppendLine("</tbody>");
			bodyBuilder.AppendLine("</table>");

			// Tổng cộng và phí vận chuyển
			bodyBuilder.AppendLine("<div style='margin-top: 20px; padding: 15px; background-color: #f9f9f9; border-radius: 8px;'>");
			bodyBuilder.AppendLine("<p style='margin: 5px 0;'><strong>Phí vận chuyển:</strong> 10,000 VNĐ</p>");
			bodyBuilder.AppendLine($"<p style='margin: 5px 0; font-size: 1.2em; color: #1a73e8;'><strong>Tổng cộng:</strong> {(hoaDon.TongTienHdb)} VNĐ</p>");
			bodyBuilder.AppendLine("</div>");

			// Thông tin bổ sung và CTA
			bodyBuilder.AppendLine("<p style='margin-top: 20px;'>Đơn hàng của bạn đang được xử lý. Bạn có thể theo dõi trạng thái đơn hàng tại:</p>");
			bodyBuilder.AppendLine($"<a href='http://localhost:3000/orders' style='display: inline-block; padding: 10px 20px; background-color: #1a73e8; color: #fff; text-decoration: none; border-radius: 4px; margin: 10px 0;'>Xem Đơn Hàng</a>");
			bodyBuilder.AppendLine("<p>Nếu bạn có thắc mắc, vui lòng liên hệ với chúng tôi qua email <a href='mailto:support@yourapp.com' style='color: #1a73e8;'>support@yourapp.com</a> hoặc số điện thoại <strong>0123-456-789</strong>.</p>");

			// Footer
			bodyBuilder.AppendLine("<div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>");
			bodyBuilder.AppendLine("<p style='color: #777; font-size: 0.9em;'>Trân trọng,<br><strong>Your App Name</strong></p>");
			bodyBuilder.AppendLine("<p style='color: #777; font-size: 0.9em;'>© 2025 Your App Name. All rights reserved.</p>");
			bodyBuilder.AppendLine("</div>");

			bodyBuilder.AppendLine("</body>");
			bodyBuilder.AppendLine("</html>");

			var mailMessage = new MailMessage
			{
				From = new MailAddress(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderName"]),
				Subject = $"Xác Nhận Đơn Hàng #{hoaDon.MaHdb}",
				Body = bodyBuilder.ToString(),
				IsBodyHtml = true
			};
			mailMessage.To.Add(toEmail);

			await smtpClient.SendMailAsync(mailMessage);
		}

	}
}