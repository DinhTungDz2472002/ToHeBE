using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HdbController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public HdbController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

			// Get All
			// Get All with Pagination
	[HttpGet]
	[Route("/HoaDonBan/GetList")]
	public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
	{
		if (pageNumber <= 0) pageNumber = 1;
		if (pageSize <= 0) pageSize = 10;

		var query = dbContext.Thdbs.AsQueryable();

		var totalItems = await query.CountAsync();
		var hdbs = await query
			.OrderByDescending(x => x.MaHdb) // Sắp xếp mới nhất trước (tùy chọn)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		var hdbDto = hdbs.Select(hdb => new HdbDto()
		{
			MaHdb = hdb.MaHdb,
			MaKhachHang = hdb.MaKhachHang,
			NgayLapHdb = hdb.NgayLapHdb,
			GiamGia = hdb.GiamGia,
			Pttt = hdb.Pttt,
			TongTienHdb = hdb.TongTienHdb
		}).ToList();

		return Ok(new
		{
			TotalItems = totalItems,
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
			Data = hdbDto
		});
	}


		// Tìm kiếm
		[HttpPost]
		[Route("/HoaDonBan/Search")]
		public async Task<IActionResult> TimKiem([FromQuery] string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return BadRequest("Từ khóa tìm kiếm không hợp lệ.");

			bool isInt = int.TryParse(s, out int number);
			DateTime? ngaySearch = DateTime.TryParse(s, out var parsedDate) ? parsedDate.Date : null;

			var hdbs = await dbContext.Thdbs.ToListAsync();

			var filtered = hdbs
				.Where(item =>
					(isInt && (item.MaHdb == number || item.MaKhachHang == number)) ||
					(ngaySearch.HasValue && item.NgayLapHdb.HasValue && item.NgayLapHdb.Value.Date == ngaySearch.Value) ||
					(item.NgayLapHdb.HasValue && (
						item.NgayLapHdb.Value.Year.ToString() == s ||
						item.NgayLapHdb.Value.Month.ToString("00") == s
					)) ||
					(!string.IsNullOrEmpty(item.Pttt) && item.Pttt.ToLower().Contains(s.ToLower()))
				)
				.ToList();

			var hdbDto = new List<HdbDto>();
			foreach (var hdb in filtered)
			{
				hdbDto.Add(new HdbDto()
				{
					MaHdb = hdb.MaHdb,
					MaKhachHang = hdb.MaKhachHang,
					NgayLapHdb = hdb.NgayLapHdb,
					GiamGia = hdb.GiamGia,
					Pttt = hdb.Pttt,
					TongTienHdb = hdb.TongTienHdb
				});
			}
			return Ok(hdbDto);
		}

		// Get by ID
		[HttpGet]
		[Route("/HoaDonBan/GetById")]
		public async Task<IActionResult> GetById([FromQuery] int id)
		{
			var hdb = await dbContext.Thdbs.FirstOrDefaultAsync(x => x.MaHdb == id);
			if (hdb == null)
				return NotFound("Không tìm thấy hóa đơn.");

			var dto = new HdbDto()
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb
			};

			return Ok(dto);
		}

		// Thêm mới
		[HttpPost]
		[Route("/HoaDonBan/Create")]
		public async Task<IActionResult> Create([FromBody] HdbDto dto)
		{
			var hdb = new Thdb()
			{
				MaKhachHang = dto.MaKhachHang,
				NgayLapHdb = dto.NgayLapHdb ?? DateTime.Now,
				GiamGia = dto.GiamGia,
				Pttt = dto.Pttt,
				TongTienHdb = dto.TongTienHdb
			};

			await dbContext.Thdbs.AddAsync(hdb);
			await dbContext.SaveChangesAsync();

			dto.MaHdb = hdb.MaHdb;

			return Ok(dto);
		}

		// Cập nhật
		[HttpPut]
		[Route("/HoaDonBan/Update")]
		public async Task<IActionResult> Update([FromBody] HdbDto dto)
		{
			var hdb = await dbContext.Thdbs.FirstOrDefaultAsync(x => x.MaHdb == dto.MaHdb);
			if (hdb == null)
				return NotFound("Không tìm thấy hóa đơn cần cập nhật.");

			hdb.MaKhachHang = dto.MaKhachHang;
			hdb.NgayLapHdb = dto.NgayLapHdb ?? hdb.NgayLapHdb;
			hdb.GiamGia = dto.GiamGia;
			hdb.Pttt = dto.Pttt;
			hdb.TongTienHdb = dto.TongTienHdb;

			await dbContext.SaveChangesAsync();

			return Ok(dto);
		}

		// Xóa
		[HttpDelete]
		[Route("/HoaDonBan/Delete")]
		public async Task<IActionResult> Delete([FromQuery] int id)
		{
			var hdb = await dbContext.Thdbs.FirstOrDefaultAsync(x => x.MaHdb == id);
			if (hdb == null)
				return NotFound("Không tìm thấy hóa đơn cần xóa.");

			dbContext.Thdbs.Remove(hdb);
			await dbContext.SaveChangesAsync();

			return Ok("Đã xóa thành công.");
		}
	}
}



