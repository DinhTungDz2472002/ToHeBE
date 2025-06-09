using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ToHeBE.Models;

namespace ToHeBE.Controllers
{
	[Route("api/ThongKeSanPham")]
	[ApiController]
	public class ThongKeSanPhamController : ControllerBase
	{
		private readonly ToHeDbContext _context;

		public ThongKeSanPhamController(ToHeDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		[Route("/api/GetAllProductSale")]
		public async Task<ActionResult> GetAllProducts()
		{
			try
			{
				// Step 1: Get delivered orders
				var deliveredOrders = _context.Thdbs
					.Where(hd => hd.Status == "Đã Giao")
					.Select(hd => hd.MaHdb);

				// Step 2: Get all products with their sales, revenue, and details
				var products = await _context.Tsanphams
					.Where(sp => sp.Status)
					.Select(sp => new
					{
						sp.MaSanPham,
						sp.TenSanPham,
						sp.GiaSanPham,
						sp.MaLoai,
						sp.SLtonKho,
						sp.AnhSp,
						sp.MoTaSp,
						sp.NgayThemSp,
						sp.Status,
						LuotBan = _context.Tchitiethdbs
							.Where(ct => ct.MaSanPham == sp.MaSanPham && deliveredOrders.Contains(ct.MaHdb))
							.Sum(ct => (int?)ct.Sl) ?? 0,
						TongTien = (_context.Tchitiethdbs
							.Where(ct => ct.MaSanPham == sp.MaSanPham && deliveredOrders.Contains(ct.MaHdb))
							.Sum(ct => (int?)ct.Sl) ?? 0) * sp.GiaSanPham,
						SoSaoTrungBinh = _context.Tdanhgias
							.Where(dg => dg.MaSanPham == sp.MaSanPham)
							.Average(dg => (double?)dg.DanhGia) ?? 0.0,
						ChiTietSps = sp.TchitietSps.Select(ct => new
						{
							ct.MaChiTietSp,
							ct.AnhChiTietSp,
							ct.GiamGiaSp
						}).ToList(),
						LoaiSanPham = sp.MaLoaiNavigation.TenLoai // Assuming Tloai has a TenLoai property
					})
					.ToListAsync();

				if (products == null || !products.Any())
				{
					return NotFound(new { message = "Không tìm thấy sản phẩm nào!" });
				}

				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi khi tải danh sách sản phẩm!", error = ex.Message });
			}
		}
	}
}