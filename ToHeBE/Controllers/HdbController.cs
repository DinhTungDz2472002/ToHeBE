using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.DTO;


namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class HdbController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public HdbController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		// Get All
		// Get All with Pagination
		// Get All with Pagination
		[HttpGet("GetChoGiaoHang")]

		public async Task<IActionResult> Get_Cho_Giao_Hang([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });

			var query = dbContext.Thdbs
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
		[HttpGet("GetChoXacNhan")]

		public async Task<IActionResult> Get_Cho_Xac_Nhan([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });

			var query = dbContext.Thdbs
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

		[HttpGet("GetDaGiao")]

		public async Task<IActionResult> Get_Da_Giao([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });

			var query = dbContext.Thdbs
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
					ThanhTien = c.ThanhTien,
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
		[HttpGet("GetDaHuy")]

		public async Task<IActionResult> Get_Da_Huy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });

			var query = dbContext.Thdbs
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

		[HttpGet("GetKhachMuonHuy")]

		public async Task<IActionResult> Get_Khach_Muon_Huy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest(new { message = "Số trang và kích thước trang phải lớn hơn 0." });

			var query = dbContext.Thdbs
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
		// Search
		[HttpPost("TimKiem")]

		public async Task<IActionResult> TimKiem([FromQuery] string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return BadRequest(new { message = "Từ khóa tìm kiếm không hợp lệ." });

			bool isInt = int.TryParse(s, out int number);
			bool isYear = int.TryParse(s, out int year) && s.Length == 4; // Check if s is a 4-digit year
			bool isMonth = int.TryParse(s, out int month) && month >= 1 && month <= 12; // Check if s is a valid month
			DateTime? ngaySearch = DateTime.TryParse(s, out var parsedDate) ? parsedDate.Date : null;

			var query = dbContext.Thdbs
				.AsNoTracking()
				.Include(x => x.Tchitiethdbs)
				.ThenInclude(c => c.MaSanPhamNavigation)
				.AsQueryable();

			query = query.Where(item =>
				(isInt && (item.MaHdb == number || item.MaKhachHang == number)) ||
				(ngaySearch.HasValue && item.NgayLapHdb.HasValue && item.NgayLapHdb.Value.Date == ngaySearch.Value) ||
				(item.NgayLapHdb.HasValue && (
					(isYear && item.NgayLapHdb.Value.Year == year) ||
					(isMonth && item.NgayLapHdb.Value.Month == month)
				)) ||
				(!string.IsNullOrEmpty(item.Pttt) && item.Pttt.ToLower().Contains(s.ToLower())) ||
				(!string.IsNullOrEmpty(item.TenKhachHang) && item.TenKhachHang.ToLower().Contains(s.ToLower())) ||
				(!string.IsNullOrEmpty(item.Sdt) && item.Sdt.ToLower().Contains(s.ToLower()))
			);

			var filtered = await query.ToListAsync();

			var hdbDto = filtered.Select(hdb => new HdbDto
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

			return Ok(new { message = "Tìm kiếm hóa đơn thành công", hoaDons = hdbDto });
		}
		
		[HttpPut("UpdateChoGiaoHang")]

		public async Task<IActionResult> UpdateChoGiaoHang([FromQuery] int maHdb)
		{
			var hdb = await dbContext.Thdbs
				.FirstOrDefaultAsync(x => x.MaHdb == maHdb);

			if (hdb == null)
				return NotFound(new { message = "Không tìm thấy hóa đơn cần cập nhật." });
			// Kiểm tra trạng thái hiện tại
			if ( hdb.Status == "Đã Giao" || hdb.Status == "Đã Hủy")
			{
				return Ok(new { message = "Không thể cập nhật trạng thái thành 'Chờ giao hàng' do trạng thái hiện tại.", currentStatus = hdb.Status });
			}

			hdb.Status = "Chờ giao hàng";

			await dbContext.SaveChangesAsync();
			return Ok(hdb);
		}

	
		[HttpPut("UpdateDaGiao")]
		public async Task<IActionResult> Update_Da_Giao([FromQuery] int maHdb)
		{
			var hdb = await dbContext.Thdbs
				.FirstOrDefaultAsync(x => x.MaHdb == maHdb);

			if (hdb == null)
				return NotFound(new { message = "Không tìm thấy hóa đơn cần cập nhật." });
			// Kiểm tra trạng thái hiện tại
			if (hdb.Status == "Đã Hủy")
			{
				return Ok(new { message = "Không thể cập nhật trạng thái thành 'Đã Giao' do trạng thái hiện tại.", currentStatus = hdb.Status });
			}
			hdb.Status = "Đã Giao";

			await dbContext.SaveChangesAsync();
			return Ok(hdb);
		}
		[HttpPut("UpdateDaHuy")]
		public async Task<IActionResult> Update_Da_Huy([FromQuery] int maHdb)
		{
			var hdb = await dbContext.Thdbs
				.FirstOrDefaultAsync(x => x.MaHdb == maHdb);

			if (hdb == null)
				return NotFound(new { message = "Không tìm thấy hóa đơn cần cập nhật." });

			hdb.Status = "Đã Hủy";

			await dbContext.SaveChangesAsync();
			return Ok(hdb);
		}

		[HttpPut("UpdateKhachMuonHuy")]
		public async Task<IActionResult> Update_Khach_Muon_Huy([FromQuery] int maHdb)
		{
			var hdb = await dbContext.Thdbs
				.FirstOrDefaultAsync(x => x.MaHdb == maHdb);
			if (hdb == null)
				return NotFound(new { message = "Không tìm thấy hóa đơn cần cập nhật." });
			if (hdb.Status == "Chờ xác nhận")
			{
				hdb.Status = "Đã Hủy";
			}
			else
			{
				hdb.Status = "Khách Muốn Hủy";
			}

			await dbContext.SaveChangesAsync();
			return Ok(hdb);
		}

	}
}



