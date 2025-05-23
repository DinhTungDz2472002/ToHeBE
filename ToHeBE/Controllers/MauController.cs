using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MauController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public MauController(ToHeDbContext context)
		{
			dbContext = context;
		}

		// GET: api/Mau
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Tmau>>> GetTmaus()
		{
			var colors = await dbContext.Tmaus.ToListAsync();
			return Ok(colors);
		}
	}
}