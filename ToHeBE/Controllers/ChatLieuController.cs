using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Controllers;
using ToHeBE.Models;
using ToHeBE.Models.DTO;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatLieuController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public ChatLieuController(ToHeDbContext context)
		{
			dbContext = context;
		}
		// GET: api/Chatlieu
		[HttpGet]
		[Route("/api/ChatLieu")]
		public async Task<ActionResult<IEnumerable<Tchatlieu>>> GetTchatlieus()
		{
			var materials = await dbContext.Tchatlieus.ToListAsync();
			return Ok(materials);
		}


		/* POST: api/Create_ChatLieu */
		[HttpPost]
		[Route("/api/Create_ChatLieu")]
		public async Task<IActionResult> CreateChatLieu([FromForm] ChatLieuDto dto)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dto.TenCl))
					return BadRequest("Tên chất liệu không được để trống.");

				var existing = await dbContext.Tchatlieus
					.FirstOrDefaultAsync(c => c.TenCl.ToLower() == dto.TenCl.Trim().ToLower());
				if (existing != null)
					return BadRequest("Tên chất liệu đã tồn tại.");

				var chatLieu = new Tchatlieu
				{
					TenCl = dto.TenCl.Trim()
				};

				dbContext.Tchatlieus.Add(chatLieu);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Thêm chất liệu thành công", maCL = chatLieu.MaCl });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		/* PUT: api/Update_ChatLieu/{maCL} */
		[HttpPut]
		[Route("/api/Update_ChatLieu/{maCL}")]
		public async Task<IActionResult> UpdateChatLieu(int maCL, [FromQuery] ChatLieuDto dto)
		{
			try
			{
				var chatLieu = await dbContext.Tchatlieus.FindAsync(maCL);
				if (chatLieu == null)
					return NotFound("Không tìm thấy chất liệu.");

				if (string.IsNullOrWhiteSpace(dto.TenCl))
					return BadRequest("Tên chất liệu không được để trống.");

				var existing = await dbContext.Tchatlieus
					.FirstOrDefaultAsync(c => c.TenCl.ToLower() == dto.TenCl.Trim().ToLower() && c.MaCl != maCL);
				if (existing != null)
					return BadRequest("Tên chất liệu đã tồn tại.");

				chatLieu.TenCl = dto.TenCl.Trim();
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Cập nhật chất liệu thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		/* DELETE: api/Delete_ChatLieu/{maCL} */
		[HttpDelete]
		[Route("/api/Delete_ChatLieu/{maCL}")]
		public async Task<IActionResult> DeleteChatLieu(int maCL)
		{
			try
			{
				var chatLieu = await dbContext.Tchatlieus
					.Include(c => c.TchitietSps)
					.FirstOrDefaultAsync(c => c.MaCl == maCL);

				if (chatLieu == null)
					return NotFound("Không tìm thấy chất liệu.");

				if (chatLieu.TchitietSps.Any())
					return BadRequest("Không thể xóa chất liệu đang được sử dụng bởi sản phẩm.");

				dbContext.Tchatlieus.Remove(chatLieu);
				await dbContext.SaveChangesAsync();

				return Ok(new { message = "Xóa chất liệu thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
	}

}
