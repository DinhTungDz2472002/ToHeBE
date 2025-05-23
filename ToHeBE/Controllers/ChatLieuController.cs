using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Controllers;
using ToHeBE.Models;

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
	}
}
