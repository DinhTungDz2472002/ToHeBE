using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ToHeBE.Models.Auth;
using ToHeBE.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DanhGiaController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public DanhGiaController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		// Endpoint: Lấy danh sách đánh giá theo sản phẩm
		[HttpGet("list_nologin")]
		[AllowAnonymous] // Cho phép truy cập công khai
		public async Task<ActionResult<IEnumerable<object>>> GetReviewsByProduct([FromQuery] int? maSanPham)
		{
			try
			{
				if (dbContext == null)
				{
					return StatusCode(500, new { error = "DbContext không được khởi tạo" });
				}

				var query = dbContext.Tdanhgias.AsQueryable();
				if (maSanPham.HasValue)
				{
					query = query.Where(d => d.MaSanPham == maSanPham);
				}

				var reviews = await query
					.Include(d => d.MaSanPhamNavigation)
					.Include(d => d.MaKhachHangNavigation) // Join với bảng KhachHang để lấy tenKhachHang
					.Select(d => new
					{
						d.MaDg,
						d.MaSanPham,
						d.MaKhachHang,
						TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.TenKhachHang : "Khách hàng ẩn danh",
						d.MaChiTietHdb,
						d.DanhGia,
						d.BinhLuan,
						d.NgayDanhGia,
						SanPham = new
						{
							d.MaSanPhamNavigation.MaSanPham,
							d.MaSanPhamNavigation.TenSanPham,
							d.MaSanPhamNavigation.AnhSp
						}
					})
					.ToListAsync();

				if (reviews == null || !reviews.Any())
				{
					return Ok(new { message = "Không tìm thấy đánh giá nào cho sản phẩm này" });
				}

				return Ok(reviews);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
				return StatusCode(500, new { error = $"Lỗi khi lấy danh sách đánh giá: {ex.Message}" });
			}
		}
		// Updated endpoint: Create a review
		// Create a review
		[Authorize]
		[HttpPost("create")]
		public async Task<ActionResult> CreateReview([FromBody] CreateDanhGiaDto createDanhGiaDto)
		{
			try
			{
				// Extract customer ID from JWT claims
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
					return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

				int maKhachHang;
				try
				{
					maKhachHang = int.Parse(userId);
				}
				catch (FormatException)
				{
					return BadRequest(new { message = "Mã khách hàng không hợp lệ" });
				}

				// Validate input
				if (!ModelState.IsValid)
					return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ" });

				// Check if the product exists
				var product = await dbContext.Tsanphams
					.AnyAsync(p => p.MaSanPham == createDanhGiaDto.MaSanPham);
				if (!product)
					return NotFound(new { message = "Sản phẩm không tồn tại" });

				// Check if the order detail exists and is from a delivered order
				var orderDetail = await dbContext.Tchitiethdbs
					.Include(ct => ct.MaHdbNavigation)
					.FirstOrDefaultAsync(ct => ct.MaChiTietHdb == createDanhGiaDto.MaChiTietHdb &&
											  ct.MaSanPham == createDanhGiaDto.MaSanPham &&
											  ct.MaHdbNavigation.MaKhachHang == maKhachHang &&
											  ct.MaHdbNavigation.Status == "Đã Giao");
				if (orderDetail == null)
					return BadRequest(new { message = "Bạn chỉ có thể đánh giá sản phẩm từ đơn hàng đã giao" });

				// Check if the customer has already reviewed this order detail
				var existingReview = await dbContext.Tdanhgias
					.AnyAsync(d => d.MaKhachHang == maKhachHang &&
								   d.MaChiTietHdb == createDanhGiaDto.MaChiTietHdb);
				if (existingReview)
					return BadRequest(new { message = "Bạn đã đánh giá chi tiết đơn hàng này rồi" });

				// Create new review
				var danhGia = new Tdanhgia
				{
					MaKhachHang = maKhachHang,
					MaSanPham = createDanhGiaDto.MaSanPham,
					MaChiTietHdb = createDanhGiaDto.MaChiTietHdb,
					DanhGia = createDanhGiaDto.DanhGia,
					BinhLuan = createDanhGiaDto.BinhLuan,
					NgayDanhGia = DateTime.UtcNow
				};

				dbContext.Tdanhgias.Add(danhGia);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Đánh giá đã được thêm thành công", danhGia});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Lỗi khi thêm đánh giá: {ex.Message}" });
			}
		}

		[HttpGet("list")]
		public async Task<ActionResult<IEnumerable<object>>> GetReviewsByCustomer([FromQuery] int? maKhachHang, [FromQuery] int? maSanPham, [FromQuery] int? maChiTietHdb)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId) && maKhachHang == null)
					return Unauthorized(new { error = "Không thể xác thực người dùng" });

				int? parsedMaKhachHang = maKhachHang;
				if (maKhachHang == null && !string.IsNullOrEmpty(userId))
				{
					try
					{
						parsedMaKhachHang = int.Parse(userId);
					}
					catch (FormatException)
					{
						return BadRequest(new { error = "Mã khách hàng không hợp lệ" });
					}
				}

				if (dbContext == null)
				{
					Console.WriteLine("DbContext is null");
					return StatusCode(500, new { error = "DbContext không được khởi tạo" });
				}

				var query = dbContext.Tdanhgias.AsQueryable();
				if (parsedMaKhachHang.HasValue)
					query = query.Where(d => d.MaKhachHang == parsedMaKhachHang);
				if (maSanPham.HasValue)
					query = query.Where(d => d.MaSanPham == maSanPham);
				if (maChiTietHdb.HasValue)
					query = query.Where(d => d.MaChiTietHdb == maChiTietHdb);

				var reviews = await query
					.Include(d => d.MaSanPhamNavigation)
					.Select(d => new
					{
						d.MaDg,
						d.MaSanPham,
						d.MaKhachHang,
						d.MaChiTietHdb,
						d.DanhGia,
						d.BinhLuan,
						d.NgayDanhGia,
						SanPham = new
						{
							d.MaSanPhamNavigation.MaSanPham,
							d.MaSanPhamNavigation.TenSanPham,
							d.MaSanPhamNavigation.AnhSp
						}
					})
					.ToListAsync();

				if (reviews == null || !reviews.Any())
				{
					return NotFound(new { error = "Không tìm thấy đánh giá nào phù hợp với tiêu chí" });
				}

				return Ok(reviews);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
				return StatusCode(500, new { error = $"Lỗi khi lấy danh sách đánh giá: {ex.Message}" });
			}
		}
		
	}
}