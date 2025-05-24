using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ToHeBE.Models.Auth;
using ToHeBE.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DanhGiaController : ControllerBase
	{
		private readonly ToHeDbContext _dbContext;

		public DanhGiaController(ToHeDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		// API thêm đánh giá sản phẩm
		[Authorize]
		[HttpPost("them_danhgia")]
		public async Task<IActionResult> AddDanhGia([FromBody] DanhGiaRequest model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Lấy thông tin khách hàng từ JWT token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { error = "Không thể xác thực người dùng" });

			int maKhachHang = int.Parse(userId);

			// Kiểm tra sản phẩm có tồn tại và trạng thái là true
			var sanPham = await _dbContext.Tsanphams.FindAsync(model.MaSanPham);
			if (sanPham == null)
				return NotFound(new { error = "Sản phẩm không tồn tại" });

			// Kiểm tra trạng thái sản phẩm
			if (sanPham.Status != true)
				return BadRequest(new { error = "Sản phẩm không ở trạng thái cho phép đánh giá" });
			

			// Kiểm tra xem khách hàng đã đánh giá sản phẩm này chưa
			var existingDanhGia = await _dbContext.Tdanhgias
				.FirstOrDefaultAsync(dg => dg.MaKhachHang == maKhachHang && dg.MaSanPham == model.MaSanPham);
			if (existingDanhGia != null)
				return BadRequest(new { error = "Bạn đã đánh giá sản phẩm này rồi" });

			// Kiểm tra điểm đánh giá hợp lệ (1-5)
			if (model.DanhGia < 1 || model.DanhGia > 5)
				return BadRequest(new { error = "Điểm đánh giá phải từ 1 đến 5" });

			// Tạo đánh giá mới
			var danhGia = new Tdanhgia
			{
				MaSanPham = model.MaSanPham,
				MaKhachHang = maKhachHang,
				DanhGia = model.DanhGia,
				BinhLuan = model.BinhLuan,
				NgayDanhGia = DateTime.Now
			};

			try
			{
				_dbContext.Tdanhgias.Add(danhGia);
				await _dbContext.SaveChangesAsync();
				return Ok(new
				{
					message = "Thêm đánh giá thành công",
					danhGia = new
					{
						danhGia.MaDg,
						danhGia.MaSanPham,
						danhGia.MaKhachHang,
						danhGia.DanhGia,
						danhGia.BinhLuan,
						danhGia.NgayDanhGia
					}
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = $"Lỗi khi thêm đánh giá: {ex.Message}" });
			}
		}
	}
}
