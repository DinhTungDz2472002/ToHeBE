using Microsoft.AspNetCore.Mvc;
using ToHeBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using ToHeBE.Models.Auth;
using System.Linq;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class KhachHangController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;
		private readonly IConfiguration _configuration;
		private readonly EmailService _emailService;

		public KhachHangController(ToHeDbContext dbContext, IConfiguration configuration, EmailService emailService)
		{
			this.dbContext = dbContext;
			_configuration = configuration;
			_emailService = emailService;
		}

		// Login API
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Kiểm tra username và mật khẩu hợp lệ
			var khachHang = dbContext.Tkhachhangs
				.FirstOrDefault(k => k.Username == model.Username);
			if (khachHang == null || !BCrypt.Net.BCrypt.Verify(model.Password, khachHang.Password))
			{
				return Unauthorized(new { error = "Thông tin đăng nhập không hợp lệ" });
			}
			// Sinh JWT Token
			var token = GenerateJwtToken(khachHang);
			return Ok(new
			{
				access_token = token,
				token_type = "bearer",
				expires_in = _configuration.GetValue<int>("Jwt:ExpiryInMinutes") * 10000,
				user = new
				{
					khachHang.MaKhachHang,
					khachHang.TenKhachHang,
					khachHang.Username,
					khachHang.Email
				}
			});
		}

		// Register API
		[HttpPost("register")]
		public IActionResult Register([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Kiểm tra nếu email hoặc username đã tồn tại trong hệ thống
			if (dbContext.Tkhachhangs.Any(k => k.Email == model.Email || k.Username == model.Username))
				return BadRequest(new { error = "Email hoặc username đã tồn tại" });

			// Thêm mới khách hàng
			var khachHang = new Tkhachhang
			{
				TenKhachHang = model.TenKhachHang,
				Username = model.Username,
				/*GioiTinh = model.GioiTinh,
				DiaChi = model.DiaChi,*/
				Sdt = model.Sdt,
				/*ChucVu = "User" // Mặc định là User khi đăng ký*/
				Email = model.Email,
				Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
			};

			dbContext.Tkhachhangs.Add(khachHang);
			dbContext.SaveChanges();

			int maKhachHang = khachHang.MaKhachHang;
			// Thêm giỏ hàng mới cho khách hàng
			var gioHang = new Tgiohang
			{
				MaKhachHang = maKhachHang
			};
			try
			{
				dbContext.Tgiohangs.Add(gioHang);
				dbContext.SaveChanges();
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}

			// Sinh JWT Token cho khách hàng sau khi đăng ký
			var token = GenerateJwtToken(khachHang);
			return Ok(new
			{
				message = "Đăng ký thành công",
				token,
				user = new
				{
					khachHang.MaKhachHang,
					khachHang.TenKhachHang,
					khachHang.Username,
					khachHang.Email
				},
				cart = new { gioHang.MaGioHang, gioHang.NgayTao }
			});
		}

		// API lấy thông tin khách hàng hiện tại (chỉ khi đã đăng nhập)
		[Authorize]
		[HttpGet("me")]
		public IActionResult Me()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var khachHang = dbContext.Tkhachhangs.Find(int.Parse(userId));
			if (khachHang == null)
				return NotFound();

			return Ok(new
			{
				khachHang.MaKhachHang,
				khachHang.TenKhachHang,
				khachHang.Email,
				khachHang.Username,
				khachHang.GioiTinh,
				khachHang.DiaChi,
				khachHang.Sdt
			});
		}

		[Authorize]
		[HttpPost("logout")]
		public IActionResult Logout()
		{
			// JWT không cần logout phía server, chỉ cần xóa token phía client
			return Ok(new { message = "Đăng xuất thành công" });
		}

		// Phương thức tạo JWT Token cho khách hàng
		private string GenerateJwtToken(Tkhachhang khachHang)
		{

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, khachHang.MaKhachHang.ToString()),
				new Claim(ClaimTypes.Name, khachHang.TenKhachHang),
				new Claim(ClaimTypes.Email, khachHang.Email),
				new Claim(ClaimTypes.NameIdentifier, khachHang.Username)
				/*new Claim(ClaimTypes.Role, user.ChucVu) // "Admin" hoặc "User"*/
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryInMinutes")),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}


		// Tạo JWT token cho khôi phục mật khẩu
		private string GenerateResetPasswordToken(string email)
		{
			var claims = new[]
			{
				new Claim(ClaimTypes.Email, email)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(1), // Token hết hạn sau 1 giờ
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		// Xác thực JWT token cho khôi phục mật khẩu
		private string ValidateResetPasswordToken(string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = _configuration["Jwt:Issuer"],
					ValidAudience = _configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(key)
				}, out SecurityToken validatedToken);

				var jwtToken = (JwtSecurityToken)validatedToken;
				return jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
			}
			catch
			{
				return null;
			}
		}

		// Forgot Password API
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var khachHang = dbContext.Tkhachhangs
				.FirstOrDefault(k => k.Email == request.Email);
			if (khachHang == null)
			{
				return BadRequest(new { error = "Email không tồn tại" });
			}

			// Tạo JWT token cho khôi phục mật khẩu
			var token = GenerateResetPasswordToken(khachHang.Email);

			try
			{
				await _emailService.SendPasswordResetEmailAsync(khachHang.Email, token);
				return Ok(new { message = "Liên kết khôi phục mật khẩu đã được gửi qua email" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = $"Lỗi khi gửi email: {ex.Message}" });
			}
		}

		// Reset Password API
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xác thực JWT token
			var email = ValidateResetPasswordToken(request.Token);
			if (email == null)
			{
				return BadRequest(new { error = "Token không hợp lệ hoặc đã hết hạn" });
			}

			var khachHang = dbContext.Tkhachhangs
				.FirstOrDefault(k => k.Email == email);
			if (khachHang == null)
			{
				return BadRequest(new { error = "Email không tồn tại" });
			}

			// Cập nhật mật khẩu
			khachHang.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
			await dbContext.SaveChangesAsync();

			return Ok(new { message = "Mật khẩu đã được đặt lại thành công" });
		}
	}
}
