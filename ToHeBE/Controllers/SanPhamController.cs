using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SanPhamController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public SanPhamController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		// 1. Lấy danh sách sản phẩm có phân trang (chỉ lấy Status = true)[
		
		[HttpGet]
		[Route("/SanPham/GetList")]
		public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
		{
			if (pageNumber <= 0 || pageSize <= 0)
				return BadRequest("Số trang và kích thước trang phải lớn hơn 0.");

			var query = dbContext.Tsanphams.Where(sp => sp.Status);

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			var sps = await query
				.OrderBy(sp => sp.MaSanPham)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var dtoList = sps.Select(sp => new SanPhamDTO
			{
				MaSanPham = sp.MaSanPham,
				TenSanPham = sp.TenSanPham,
				GiaSanPham = sp.GiaSanPham,
				MaLoai = sp.MaLoai,
				SLtonKho = sp.SLtonKho,
				AnhSp = sp.AnhSp,
				MoTaSp = sp.MoTaSp,
				NgayThemSp = sp.NgayThemSp,
				Status = sp.Status
			}).ToList();

			var response = new
			{
				CurrentPage = pageNumber,
				PageSize = pageSize,
				TotalItems = totalItems,
				TotalPages = totalPages,
				Items = dtoList
			};

			return Ok(response);
		}


		// 2. Tìm kiếm sản phẩm
		[HttpPost]
		[Route("/SanPham/Search")]
		public async Task<IActionResult> Search([FromQuery] string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return BadRequest("Từ khóa tìm kiếm không hợp lệ.");

			bool isInt = int.TryParse(s, out int number);

			var sps = await dbContext.Tsanphams.Where(sp => sp.Status).ToListAsync(); ;

			var filtered = sps.Where(sp =>
				(isInt && sp.MaSanPham == number) ||
				(!string.IsNullOrEmpty(sp.TenSanPham) && sp.TenSanPham.ToLower().Contains(s.ToLower())) ||
				(!string.IsNullOrEmpty(sp.MoTaSp) && sp.MoTaSp.ToLower().Contains(s.ToLower()))
			).ToList();

			var dtoList = filtered.Select(sp => new SanPhamDTO
			{
				MaSanPham = sp.MaSanPham,
				TenSanPham = sp.TenSanPham,
				GiaSanPham = sp.GiaSanPham,
				MaLoai = sp.MaLoai,
				SLtonKho = sp.SLtonKho,
				AnhSp = sp.AnhSp,
				MoTaSp = sp.MoTaSp,
				NgayThemSp = sp.NgayThemSp,
				Status = sp.Status
			}).ToList();

			return Ok(dtoList);
		}
		// 3. Lấy sản phẩm theo ID (chỉ lấy Status = true)
		[HttpGet]
		[Route("/SanPham/GetById")]
		public async Task<IActionResult> GetById([FromQuery] int id)
		{
			var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == id && x.Status);
			if (sp == null)
				return NotFound("Không tìm thấy sản phẩm.");

			var dto = new SanPhamDTO
			{
				MaSanPham = sp.MaSanPham,
				TenSanPham = sp.TenSanPham,
				GiaSanPham = sp.GiaSanPham,
				MaLoai = sp.MaLoai,
				SLtonKho = sp.SLtonKho,
				AnhSp = sp.AnhSp,
				MoTaSp = sp.MoTaSp,
				NgayThemSp = sp.NgayThemSp,
				Status = sp.Status
			};

			return Ok(dto);
		}
		// 4. Thêm mới sản phẩm (mặc định Status = true)
		[HttpPost]
		[Route("/SanPham/Create")]
		public async Task<IActionResult> Create([FromBody] SanPhamDTO dto)
		{
			var sp = new Tsanpham
			{
				TenSanPham = dto.TenSanPham,
				GiaSanPham = dto.GiaSanPham,
				MaLoai = dto.MaLoai,
				SLtonKho = dto.SLtonKho,
				AnhSp = dto.AnhSp,
				MoTaSp = dto.MoTaSp,
				NgayThemSp = dto.NgayThemSp ?? DateTime.Now,
				Status = true
			};

			await dbContext.Tsanphams.AddAsync(sp);
			await dbContext.SaveChangesAsync();

			dto.MaSanPham = sp.MaSanPham;
			dto.Status = true;

			return Ok(dto);
		}

		// 5. Cập nhật sản phẩm
		[HttpPut]
		[Route("/SanPham/Update")]
		public async Task<IActionResult> Update([FromBody] SanPhamDTO dto)
		{
			var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == dto.MaSanPham && x.Status);
			if (sp == null)
				return NotFound("Không tìm thấy sản phẩm cần cập nhật.");

			sp.TenSanPham = dto.TenSanPham;
			sp.GiaSanPham = dto.GiaSanPham;
			sp.MaLoai = dto.MaLoai;
			sp.SLtonKho = dto.SLtonKho;
			sp.AnhSp = dto.AnhSp;
			sp.MoTaSp = dto.MoTaSp;
			sp.NgayThemSp = dto.NgayThemSp ?? sp.NgayThemSp;

			await dbContext.SaveChangesAsync();

			return Ok(dto);
		}

		// 6. "Xóa" sản phẩm (chuyển Status = false)
		[HttpDelete]
		[Route("/SanPham/Delete")]
		public async Task<IActionResult> Delete([FromQuery] int id)
		{
			var sp = await dbContext.Tsanphams.FirstOrDefaultAsync(x => x.MaSanPham == id && x.Status);
			if (sp == null)
				return NotFound("Không tìm thấy sản phẩm cần xóa.");

			sp.Status = false; // Chuyển trạng thái
			await dbContext.SaveChangesAsync();

			return Ok("Đã xóa sản phẩm thành công.");
		}
	}


}
