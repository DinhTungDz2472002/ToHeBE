using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToHeBE.Models.Auth;
using ToHeBE.Models;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DsOrder_UserController : ControllerBase
	{

		private readonly ToHeDbContext dbContext;


		public DsOrder_UserController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
			
		}
		// Get All
		[HttpGet("GetChoGiaoHang")]
		public async Task<IActionResult> Get_Cho_Giao_Hang([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			var query = dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			// Lọc hóa đơn có Status là "Chờ giao hàng"
			query = query.Where(x => x.Status == "Chờ giao hàng");
			var hdbs = await query
				.OrderByDescending(x => x.NgayLapHdb ?? DateTime.MinValue)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();


			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb ?? 0, // Default to 0 if null
				Status = hdb.Status,
				TenKhachHang = hdb.TenKhachHang,
				DiaChi = hdb.DiaChi,
				Sdt = hdb.Sdt,
				ChiTietHoaDon = hdb.Tchitiethdbs.Select(c => new ChiTietHdbDto
				{
					MaSanPham = c.MaSanPham,
					Sl = c.Sl,
					ThanhTien = c.ThanhTien ?? 0, // Default to 0 if null
					TenSanPham = c.MaSanPhamNavigation?.TenSanPham,
					AnhSp = c.MaSanPhamNavigation?.AnhSp,
					GiaSanPham = c.MaSanPhamNavigation?.GiaSanPham
				}).ToList()
			}).ToList();

			if (!hdbDto.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new
			{
				message = "Lấy danh sách hóa đơn thành công",
				currentPage = pageNumber,
				pageSize = pageSize,
				totalItems = totalItems,
				totalPages = totalPages,
				hoaDons = hdbDto
			});
		}

		// Get All
		[HttpGet("GetChoXacNhan")]
		public async Task<IActionResult> Get_Cho_Xac_Nhan([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			var query = dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			// Lọc hóa đơn có Status là "Chờ giao hàng"
			query = query.Where(x => x.Status == "Chờ xác nhận");
			var hdbs = await query
				.OrderByDescending(x => x.NgayLapHdb ?? DateTime.MinValue)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();


			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb ?? 0, // Default to 0 if null
				Status = hdb.Status,
				TenKhachHang = hdb.TenKhachHang,
				DiaChi = hdb.DiaChi,
				Sdt = hdb.Sdt,
				ChiTietHoaDon = hdb.Tchitiethdbs.Select(c => new ChiTietHdbDto
				{
					MaSanPham = c.MaSanPham,
					Sl = c.Sl,
					ThanhTien = c.ThanhTien ?? 0, // Default to 0 if null
					TenSanPham = c.MaSanPhamNavigation?.TenSanPham,
					AnhSp = c.MaSanPhamNavigation?.AnhSp,
					GiaSanPham = c.MaSanPhamNavigation?.GiaSanPham
				}).ToList()
			}).ToList();

			if (!hdbDto.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new
			{
				message = "Lấy danh sách hóa đơn thành công",
				currentPage = pageNumber,
				pageSize = pageSize,
				totalItems = totalItems,
				totalPages = totalPages,
				hoaDons = hdbDto
			});
		}


		// Get All
		[HttpGet("GetDaGiao")]
		public async Task<IActionResult> Get_Da_Giao([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			var query = dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			// Lọc hóa đơn có Status là "Chờ giao hàng"
			query = query.Where(x => x.Status == "Đã Giao");
			var hdbs = await query
				.OrderByDescending(x => x.NgayLapHdb ?? DateTime.MinValue)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();


			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb ?? 0, // Default to 0 if null
				Status = hdb.Status,
				TenKhachHang = hdb.TenKhachHang,
				DiaChi = hdb.DiaChi,
				Sdt = hdb.Sdt,
				ChiTietHoaDon = hdb.Tchitiethdbs.Select(c => new ChiTietHdbDto
				{
					MaSanPham = c.MaSanPham,
					Sl = c.Sl,
					ThanhTien = c.ThanhTien ?? 0, // Default to 0 if null
					TenSanPham = c.MaSanPhamNavigation?.TenSanPham,
					AnhSp = c.MaSanPhamNavigation?.AnhSp,
					GiaSanPham = c.MaSanPhamNavigation?.GiaSanPham
				}).ToList()
			}).ToList();

			if (!hdbDto.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new
			{
				message = "Lấy danh sách hóa đơn thành công",
				currentPage = pageNumber,
				pageSize = pageSize,
				totalItems = totalItems,
				totalPages = totalPages,
				hoaDons = hdbDto
			});
		}


		// Get All
		[HttpGet("GetKhachMuonHuy")]
		public async Task<IActionResult> Get_Khach_Muon_Huy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			var query = dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			// Lọc hóa đơn có Status là "Chờ giao hàng"
			query = query.Where(x => x.Status == "Khách Muốn Hủy");
			var hdbs = await query
				.OrderByDescending(x => x.NgayLapHdb ?? DateTime.MinValue)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();


			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb ?? 0, // Default to 0 if null
				Status = hdb.Status,
				TenKhachHang = hdb.TenKhachHang,
				DiaChi = hdb.DiaChi,
				Sdt = hdb.Sdt,
				ChiTietHoaDon = hdb.Tchitiethdbs.Select(c => new ChiTietHdbDto
				{
					MaSanPham = c.MaSanPham,
					Sl = c.Sl,
					ThanhTien = c.ThanhTien ?? 0, // Default to 0 if null
					TenSanPham = c.MaSanPhamNavigation?.TenSanPham,
					AnhSp = c.MaSanPhamNavigation?.AnhSp,
					GiaSanPham = c.MaSanPhamNavigation?.GiaSanPham
				}).ToList()
			}).ToList();

			if (!hdbDto.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new
			{
				message = "Lấy danh sách hóa đơn thành công",
				currentPage = pageNumber,
				pageSize = pageSize,
				totalItems = totalItems,
				totalPages = totalPages,
				hoaDons = hdbDto
			});
		}


		// Get All
		[HttpGet("GetDaHuy")]
		public async Task<IActionResult> Get_Da_Huy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });
			// Lấy ID khách hàng từ token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });

			var query = dbContext.Thdbs
				.Where(h => h.MaKhachHang == int.Parse(userId))
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			// Lọc hóa đơn có Status là "Chờ giao hàng"
			query = query.Where(x => x.Status == "Đã Hủy");
			var hdbs = await query
				.OrderByDescending(x => x.NgayLapHdb ?? DateTime.MinValue)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();


			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb ?? 0, // Default to 0 if null
				Status = hdb.Status,
				TenKhachHang = hdb.TenKhachHang,
				DiaChi = hdb.DiaChi,
				Sdt = hdb.Sdt,
				ChiTietHoaDon = hdb.Tchitiethdbs.Select(c => new ChiTietHdbDto
				{
					MaSanPham = c.MaSanPham,
					Sl = c.Sl,
					ThanhTien = c.ThanhTien ?? 0, // Default to 0 if null
					TenSanPham = c.MaSanPhamNavigation?.TenSanPham,
					AnhSp = c.MaSanPhamNavigation?.AnhSp,
					GiaSanPham = c.MaSanPhamNavigation?.GiaSanPham
				}).ToList()
			}).ToList();

			if (!hdbDto.Any())
				return Ok(new { message = "Không có hóa đơn nào", hoaDons = new List<object>() });

			return Ok(new
			{
				message = "Lấy danh sách hóa đơn thành công",
				currentPage = pageNumber,
				pageSize = pageSize,
				totalItems = totalItems,
				totalPages = totalPages,
				hoaDons = hdbDto
			});
		}


	}
}
