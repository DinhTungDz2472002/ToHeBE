using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
		//Get ALL
		[HttpGet]
		[Route("/HoaDonban/GetList")]
		public IActionResult GetAll()
		{
			//Get Data from Database - Domain models
			var hdbs = dbContext.Thdbs.ToList();

			//map domain Models to DTOs
			var hdbDto = new List<HdbDto>();
			foreach (var hdb in hdbs) {
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
			//return DTOs
			return Ok(hdbDto);
		}
		//Get id
		[HttpPost]
		[Route("/HoaDonBan/TimKiem")]
		public IActionResult TimKiem([FromQuery] string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return BadRequest("Từ khóa tìm kiếm không hợp lệ.");

			// Kiểm tra nếu s là số nguyên
			bool isInt = int.TryParse(s, out int number);

			// Kiểm tra nếu s là ngày hợp lệ
			DateTime? ngaySearch = null;
			if (DateTime.TryParse(s, out var parsedDate))
				ngaySearch = parsedDate.Date;

			// Truy vấn dữ liệu
			var hdbs = dbContext.Thdbs
			.AsEnumerable() // Chuyển sang LINQ to Object để dùng ToString
			.Where(item =>
				(isInt && (item.MaHdb == number || item.MaKhachHang == number)) ||

				(ngaySearch.HasValue && item.NgayLapHdb.HasValue && item.NgayLapHdb.Value.Date == ngaySearch.Value) ||

				(item.NgayLapHdb.HasValue && (
					item.NgayLapHdb.Value.Year.ToString() == s ||
					item.NgayLapHdb.Value.Month.ToString("00") == s
				)) ||

				(!string.IsNullOrEmpty(item.Pttt) && item.Pttt.ToLower().Contains(s.ToLower()))
			).ToList();


			// map domain models to DTOs
			var hdbDto = hdbs.Select(hdb => new HdbDto
			{
				MaHdb = hdb.MaHdb,
				MaKhachHang = hdb.MaKhachHang,
				NgayLapHdb = hdb.NgayLapHdb,
				GiamGia = hdb.GiamGia,
				Pttt = hdb.Pttt,
				TongTienHdb = hdb.TongTienHdb
			}).ToList();

			return Ok(hdbDto);
		}



	}
}
