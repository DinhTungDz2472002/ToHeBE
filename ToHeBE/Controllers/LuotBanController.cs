using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ToHeBE.Models;

namespace ToHeBE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LuotBanController : ControllerBase
	{
		private readonly ToHeDbContext _context;

		public LuotBanController(ToHeDbContext context)
		{
			_context = context;
		}

		// GET: api/LuotBan/{maSanPham}
		[HttpGet("{maSanPham}")]
		public async Task<ActionResult> GetProductSalesById(int maSanPham)
		{
			try
			{
				// Step 1: Get delivered orders
				var deliveredOrders = _context.Thdbs
					.Where(hd => hd.Status == "Đã Giao")
					.Select(hd => hd.MaHdb);

				// Step 2: Get sales count for the specific product
				var sales = await _context.Tchitiethdbs
					.Where(ct => ct.MaSanPham == maSanPham && deliveredOrders.Contains(ct.MaHdb))
					.SumAsync(ct => (int?)ct.Sl) ?? 0;

				// Step 3: Get product details and average rating
				var product = await _context.Tsanphams
					.Where(sp => sp.MaSanPham == maSanPham && sp.Status)
					.Select(sp => new
					{
						sp.MaSanPham,
						sp.TenSanPham,
						sp.GiaSanPham,
						sp.AnhSp,
						sp.MoTaSp,
						LuotBan = sales,
						SoSaoTrungBinh = _context.Tdanhgias
							.Where(dg => dg.MaSanPham == sp.MaSanPham)
							.Average(dg => (double?)dg.DanhGia) ?? 0.0,
						ChiTietSps = sp.TchitietSps.Select(ct => new
						{
							ct.MaChiTietSp,
							ct.AnhChiTietSp,
							ct.GiamGiaSp,
							
						}).ToList()
					})
					.FirstOrDefaultAsync();

				if (product == null)
				{
					return NotFound(new { message = "Không tìm thấy sản phẩm!" });
				}

				return Ok(product);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi khi tải chi tiết sản phẩm!", error = ex.Message });
			}
		}
	}
}