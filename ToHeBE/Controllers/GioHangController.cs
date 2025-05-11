using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToHeBE.Models;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GioHangController : ControllerBase
	{
		private readonly ToHeDbContext dbContext;

		public GioHangController(ToHeDbContext dbContext) {
			this.dbContext = dbContext;
		}

	}
}
