using Microsoft.AspNetCore.Mvc;
using ToHeBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ToHeBE.Models.Auth;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class GioHangController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public GioHangController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		[HttpGet]
		public IActionResult GetGioHang()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var gioHang = dbContext.Tgiohangs
				.Include(g => g.Tchitietgiohangs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.FirstOrDefault(g => g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
				return NotFound(new { message = "Giỏ hàng không tồn tại" });

			var chiTiet = gioHang.Tchitietgiohangs
				.Where(c => c.MaSanPhamNavigation.Status == true)
				.Select(c => new
			{
				c.MaChiTietGH,
				c.MaSanPham,
				SanPham = c.MaSanPhamNavigation.TenSanPham,
				AnhSp = c.MaSanPhamNavigation.AnhSp,
				GiaSanPham = c.MaSanPhamNavigation.GiaSanPham,
				c.SlSP,
				c.DonGia
			}).ToList();

			return Ok(new
			{
				gioHang.MaGioHang,
				gioHang.NgayTao,
				ChiTietGioHang = chiTiet,
				TongTien = chiTiet.Sum(c => c.DonGia)
			});
		}


		/*[HttpPost("them-san-pham")]
		public IActionResult ThemSanPham([FromBody] ThemSanPhamCartModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var gioHang = dbContext.Tgiohangs
				.FirstOrDefault(g => g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
			{
				gioHang = new Tgiohang
				{
					MaKhachHang = int.Parse(userId),
					NgayTao = DateTime.UtcNow
				};
				dbContext.Tgiohangs.Add(gioHang);
				dbContext.SaveChanges();
			}

			var sanPham = dbContext.Tsanphams.Find(model.MaSanPham);
			if (sanPham == null)
				return NotFound(new { message = "Sản phẩm không tồn tại" });

			var chiTiet = dbContext.Tchitietgiohangs
				.FirstOrDefault(c => c.MaGioHang == gioHang.MaGioHang && c.MaSanPham == model.MaSanPham);

			if (chiTiet != null)
			{
				chiTiet.SlSP += model.SlSP;
				chiTiet.DonGia = (double)sanPham.GiaSanPham * chiTiet.SlSP;
			}
			else
			{
				chiTiet = new Tchitietgiohang
				{
					MaGioHang = gioHang.MaGioHang,
					MaSanPham = model.MaSanPham,
					SlSP = model.SlSP,
					DonGia = (double)sanPham.GiaSanPham * model.SlSP
				};
				dbContext.Tchitietgiohangs.Add(chiTiet);
			}

			dbContext.SaveChanges();

			return Ok(new { message = "Thêm sản phẩm thành công" });
		}
*/
		[HttpPost("them-san-pham")]
		public IActionResult ThemSanPham([FromBody] ThemSanPhamCartModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var gioHang = dbContext.Tgiohangs
				.FirstOrDefault(g => g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
			{
				gioHang = new Tgiohang
				{
					MaKhachHang = int.Parse(userId),
					NgayTao = DateTime.UtcNow
				};
				dbContext.Tgiohangs.Add(gioHang);
				dbContext.SaveChanges();
			}

			var sanPham = dbContext.Tsanphams.Find(model.MaSanPham);
			if (sanPham == null)
				return NotFound(new { message = "Sản phẩm không tồn tại" });

			var chiTiet = dbContext.Tchitietgiohangs
				.FirstOrDefault(c => c.MaGioHang == gioHang.MaGioHang && c.MaSanPham == model.MaSanPham);

			if (chiTiet != null)
			{
				chiTiet.SlSP += model.SlSP;
				chiTiet.DonGia = (double)sanPham.GiaSanPham * chiTiet.SlSP;
			}
			else
			{
				chiTiet = new Tchitietgiohang
				{
					MaGioHang = gioHang.MaGioHang,
					MaSanPham = model.MaSanPham,
					SlSP = model.SlSP,
					DonGia = (double)sanPham.GiaSanPham * model.SlSP
				};
				dbContext.Tchitietgiohangs.Add(chiTiet);
			}

			dbContext.SaveChanges();

			// Trả về thông tin chi tiết của sản phẩm vừa thêm
			return Ok(new
			{
				message = "Thêm sản phẩm thành công",
				chiTietGioHang = new
				{
					MaChiTietGH = chiTiet.MaChiTietGH, // Giả sử Tchitietgiohang có thuộc tính MaChiTietGH
					MaSanPham = chiTiet.MaSanPham,
					SlSP = chiTiet.SlSP,
					DonGia = chiTiet.DonGia,
					SanPham = new
					{
						TenSanPham = sanPham.TenSanPham,
						AnhSp = sanPham.AnhSp,
						GiaSanPham = sanPham.GiaSanPham
					}
				}
			});
		}


		[HttpDelete("xoa-san-pham/{maChiTietGH}")]
		public IActionResult XoaSanPham(int maChiTietGH)
		{
			var chiTiet = dbContext.Tchitietgiohangs.Find(maChiTietGH);
			if (chiTiet == null)
				return NotFound(new { message = "Chi tiết giỏ hàng không tồn tại" });

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var gioHang = dbContext.Tgiohangs
				.FirstOrDefault(g => g.MaGioHang == chiTiet.MaGioHang && g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
				return Unauthorized(new { message = "Không có quyền xóa" });

			dbContext.Tchitietgiohangs.Remove(chiTiet);
			dbContext.SaveChanges();

			return Ok(new { message = "Xóa sản phẩm thành công" });
		}
		[HttpPut("cap-nhat-so-luong/{maChiTietGH}")]
		public IActionResult CapNhatSoLuong(int maChiTietGH, [FromBody] CapNhatSoLuongCartModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var chiTiet = dbContext.Tchitietgiohangs.Find(maChiTietGH);
			if (chiTiet == null)
				return NotFound(new { message = "Chi tiết giỏ hàng không tồn tại" });

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var gioHang = dbContext.Tgiohangs
				.FirstOrDefault(g => g.MaGioHang == chiTiet.MaGioHang && g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
				return Unauthorized(new { message = "Không có quyền cập nhật" });

			chiTiet.SlSP = model.SlSP;
			chiTiet.DonGia = chiTiet.DonGia / chiTiet.SlSP * model.SlSP; // Cập nhật đơn giá dựa trên số lượng mới
			dbContext.SaveChanges();

			return Ok(new { message = "Cập nhật số lượng thành công" });
		}

	}

}
