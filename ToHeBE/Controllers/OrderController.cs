using Microsoft.AspNetCore.Mvc;
using ToHeBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToHeBE.Models.Auth;
namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class OrderController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;
		private readonly EmailService _emailService;

		public OrderController(ToHeDbContext dbContext, EmailService emailService)
		{
			this.dbContext = dbContext;
			_emailService = emailService;
		}


		[HttpPost("tao-hoa-don")]
		public async Task<IActionResult> TaoHoaDon([FromBody] TaoHoaDonModel model)
		{
			// Kiểm tra dữ liệu đầu vào
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			// Lấy thông tin khách hàng
			var khachHang = await dbContext.Tkhachhangs.FindAsync(int.Parse(userId));
			if (khachHang == null)
				return NotFound(new { message = "Khách hàng không tồn tại" });

			// Kiểm tra danh sách sản phẩm được chọn
			if (model.SelectedItems == null || !model.SelectedItems.Any())
				return BadRequest(new { message = "Không có sản phẩm nào được chọn" });

			// Lấy giỏ hàng của khách hàng
			var gioHang = await dbContext.Tgiohangs
				.Include(g => g.Tchitietgiohangs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.FirstOrDefaultAsync(g => g.MaKhachHang == int.Parse(userId));

			if (gioHang == null)
				return BadRequest(new { message = "Giỏ hàng không tồn tại" });

			// Kiểm tra các sản phẩm được chọn
			var chiTietGioHang = gioHang.Tchitietgiohangs
				.Where(c => model.SelectedItems.Select(s => s.MaChiTietGH).Contains(c.MaChiTietGH))
				.ToList();

			if (chiTietGioHang.Count != model.SelectedItems.Count)
				return BadRequest(new { message = "Một số sản phẩm được chọn không hợp lệ hoặc không tồn tại trong giỏ hàng" });

			// Kiểm tra số lượng và tồn kho
			foreach (var selectedItem in model.SelectedItems)
			{
				var chiTiet = chiTietGioHang.FirstOrDefault(c => c.MaChiTietGH == selectedItem.MaChiTietGH);
				if (chiTiet == null)
					return BadRequest(new { message = $"Chi tiết giỏ hàng {selectedItem.MaChiTietGH} không tồn tại" });

				if (chiTiet.MaSanPhamNavigation.Status == false)
					return BadRequest(new { message = $"Sản phẩm {chiTiet.MaSanPhamNavigation.TenSanPham} không còn hoạt động" });

				if (selectedItem.SlSP > chiTiet.MaSanPhamNavigation.SLtonKho)
					return BadRequest(new { message = $"Sản phẩm {chiTiet.MaSanPhamNavigation.TenSanPham} không đủ số lượng tồn kho" });

				// Cập nhật số lượng trong chi tiết giỏ hàng
				chiTiet.SlSP = selectedItem.SlSP;
				chiTiet.DonGia = (double)chiTiet.MaSanPhamNavigation.GiaSanPham * selectedItem.SlSP;
			}

			// Tạo hóa đơn bán
			using var transaction = await dbContext.Database.BeginTransactionAsync();
			try
			{
				var hoaDon = new Thdb
				{
					MaKhachHang = int.Parse(userId),
					NgayLapHdb = DateTime.UtcNow,
					DiaChi = model.DiaChi,
					Sdt = model.SDT,
					TenKhachHang = model.TenKhachHang,
					Status = "Chờ xác nhận",
					GiamGia = 0, // Có thể thêm logic tính giảm giá nếu cần
					TongTienHdb = chiTietGioHang.Sum(c => c.DonGia)
				};

				dbContext.Thdbs.Add(hoaDon);
				await dbContext.SaveChangesAsync();

				var chiTietHdbList = new List<Tchitiethdb>();
				// Tạo chi tiết hóa đơn
				foreach (var chiTiet in chiTietGioHang)
				{
					var chiTietHDB = new Tchitiethdb
					{
						MaHdb = hoaDon.MaHdb,
						MaSanPham = chiTiet.MaSanPham,
						Sl = chiTiet.SlSP,
						ThanhTien = chiTiet.DonGia
					};
					chiTietHdbList.Add(chiTietHDB);
					dbContext.Tchitiethdbs.Add(chiTietHDB);

					// Cập nhật số lượng tồn kho
					var sanPham = chiTiet.MaSanPhamNavigation;
					sanPham.SLtonKho -= chiTiet.SlSP;
				}

				// Xóa các chi tiết giỏ hàng được chọn
				dbContext.Tchitietgiohangs.RemoveRange(chiTietGioHang);
				await dbContext.SaveChangesAsync();

				// Gửi email xác nhận đơn hàng
				try
				{
					await _emailService.SendOrderConfirmationEmailAsync(
						khachHang.Email,
						hoaDon,
						chiTietHdbList,
						model.TenKhachHang
					);
				}
				catch (Exception ex)
				{
					// Log lỗi gửi email nhưng không làm thất bại giao dịch
					Console.WriteLine($"Lỗi gửi email xác nhận đơn hàng: {ex.Message}");
				}

				await transaction.CommitAsync();

				return Ok(new
				{
					message = "Tạo hóa đơn thành công. Hóa đơn đã được gửi qua email.",
					hoaDon = new
					{
						hoaDon.MaHdb,
						hoaDon.NgayLapHdb,
						hoaDon.TongTienHdb,
						hoaDon.DiaChi,
						hoaDon.Sdt,
						hoaDon.Status,
						ChiTietHoaDon = chiTietHdbList.Select(c => new
						{
							c.MaSanPham,
							c.Sl,
							c.ThanhTien,
							TenSanPham = c.MaSanPhamNavigation.TenSanPham,
							AnhSp = c.MaSanPhamNavigation.AnhSp
						})
					}
				});
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, new { message = "Lỗi khi tạo hóa đơn", error = ex.Message });
			}
		}
		// Endpoint mới để lấy danh sách hóa đơn
		[HttpGet]
		public async Task<IActionResult> GetList()
		{
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			// Lấy danh sách hóa đơn của khách hàng
			var hoaDons = await dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.Include(h => h.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.OrderByDescending(h => h.NgayLapHdb) // Sắp xếp hóa đơn mới nhất lên đầu
				.Select(h => new
				{
					h.MaHdb,
					h.NgayLapHdb,
					h.TongTienHdb,
					h.TenKhachHang,
					h.DiaChi,
					h.Sdt,
					h.Status,
					ChiTietHoaDon = h.Tchitiethdbs.Select(c => new
					{
						c.MaSanPham,
						c.Sl,
						c.ThanhTien,
						TenSanPham = c.MaSanPhamNavigation.TenSanPham,
						AnhSp = c.MaSanPhamNavigation.AnhSp,
						GiaSanPham = c.MaSanPhamNavigation.GiaSanPham
					}).ToList()
				})
				.ToListAsync();

			if (!hoaDons.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new { message = "Lấy danh sách hóa đơn thành công", hoaDons });
		}
	
}
}