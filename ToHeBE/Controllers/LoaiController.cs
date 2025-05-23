using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;

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
	}
}
