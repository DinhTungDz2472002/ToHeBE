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

		public KhachHangController(ToHeDbContext dbContext, IConfiguration configuration)
		{
			this.dbContext = dbContext;
			_configuration = configuration;
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
	}
}
