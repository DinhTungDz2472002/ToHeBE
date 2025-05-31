/*using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.Auth;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoaiController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public LoaiController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		*//*get list loại*//*
		[HttpGet]
		[Route("/api/Loai")]
		public async Task<IActionResult> GetAllLoai()
		{
			try
			{
				var loaiList = await dbContext.Tloais
					.Select(l => new { l.MaLoai, l.TenLoai })
					.ToListAsync();
				return Ok(loaiList);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}



		*//*create loại*//*
		[HttpPost]
		[Route("/api/Create_Loai")]
		public async Task<IActionResult> CreateLoai([FromForm] LoaiDto dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.TenLoai))
					return BadRequest("Tên loại không được để trống.");

				var loai = new Tloai
				{
					TenLoai = dto.TenLoai
				};

				dbContext.Tloais.Add(loai);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Thêm loại thành công", maLoai = loai.MaLoai });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		*//*update loại*//*
		[HttpPut]
		[Route("/api/Update_Loai/{maLoai}")]
		public async Task<IActionResult> UpdateLoai(int maLoai, [FromQuery] LoaiDto dto)
		{
			try
			{
				var loai = await dbContext.Tloais.FindAsync(maLoai);
				if (loai == null)
					return NotFound("Không tìm thấy loại.");

				if (string.IsNullOrWhiteSpace(dto.TenLoai))
					return BadRequest("Tên loại không được để trống.");

				loai.TenLoai = dto.TenLoai;
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Cập nhật loại thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		*//*delete loại*//*
		[HttpDelete]
		[Route("/api/Delete_Loai/{maLoai}")]
		public async Task<IActionResult> DeleteLoai(int maLoai)
		{
			try
			{
				var loai = await dbContext.Tloais
					.Include(l => l.Tsanphams)
					.FirstOrDefaultAsync(l => l.MaLoai == maLoai);

				if (loai == null)
					return NotFound("Không tìm thấy loại.");

				if (loai.Tsanphams.Any())
					return BadRequest("Không thể xóa loại đang được sử dụng bởi sản phẩm.");

				dbContext.Tloais.Remove(loai);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Xóa loại thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

	}
}
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using ToHeBE.Models.Auth;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoaiController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public LoaiController(ToHeDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		/*get list loại*/
		[HttpGet]
		[Route("/api/Loai")]
		public async Task<IActionResult> GetAllLoai()
		{
			try
			{
				var loaiList = await dbContext.Tloais
					.Select(l => new { l.MaLoai, l.TenLoai })
					.ToListAsync();
				return Ok(loaiList);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		/*create loại*/
		[HttpPost]
		[Route("/api/Create_Loai")]
		public async Task<IActionResult> CreateLoai([FromForm] LoaiDto dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.TenLoai))
					return BadRequest("Tên loại không được để trống.");

				// Check for duplicate TenLoai
				var existingLoai = await dbContext.Tloais
					.FirstOrDefaultAsync(l => l.TenLoai.ToLower() == dto.TenLoai.Trim().ToLower());
				if (existingLoai != null)
					return BadRequest("Tên loại đã tồn tại.");

				var loai = new Tloai
				{
					TenLoai = dto.TenLoai.Trim()
				};

				dbContext.Tloais.Add(loai);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Thêm loại thành công", maLoai = loai.MaLoai });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		/*update loại*/
		[HttpPut]
		[Route("/api/Update_Loai/{maLoai}")]
		public async Task<IActionResult> UpdateLoai(int maLoai, [FromQuery] LoaiDto dto)
		{
			try
			{
				var loai = await dbContext.Tloais.FindAsync(maLoai);
				if (loai == null)
					return NotFound("Không tìm thấy loại.");

				if (string.IsNullOrWhiteSpace(dto.TenLoai))
					return BadRequest("Tên loại không được để trống.");

				// Check for duplicate TenLoai, excluding the current Loai
				var existingLoai = await dbContext.Tloais
					.FirstOrDefaultAsync(l => l.TenLoai.ToLower() == dto.TenLoai.Trim().ToLower() && l.MaLoai != maLoai);
				if (existingLoai != null)
					return BadRequest("Tên loại đã tồn tại.");

				loai.TenLoai = dto.TenLoai.Trim();
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Cập nhật loại thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		/*delete loại*/
		[HttpDelete]
		[Route("/api/Delete_Loai/{maLoai}")]
		public async Task<IActionResult> DeleteLoai(int maLoai)
		{
			try
			{
				var loai = await dbContext.Tloais
					.Include(l => l.Tsanphams)
					.FirstOrDefaultAsync(l => l.MaLoai == maLoai);

				if (loai == null)
					return NotFound("Không tìm thấy loại.");

				if (loai.Tsanphams.Any())
					return BadRequest("Không thể xóa loại đang được sử dụng bởi sản phẩm.");

				dbContext.Tloais.Remove(loai);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Xóa loại thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
	}
}