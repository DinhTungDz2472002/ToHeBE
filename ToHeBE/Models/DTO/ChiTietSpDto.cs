using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class ChiTietSpDto
	{
		[Column("maChiTietSP")]
		public int MaChiTietSp { get; set; }

		[Column("maSanPham")]
		public int MaSanPham { get; set; }

		[Column("maMau")]
		[Required(ErrorMessage = "Màu sắc là bắt buộc")]
		public int MaMau { get; set; }

		[Column("maCL")]
		[Required(ErrorMessage = "Chất liệu là bắt buộc")]
		public int MaCl { get; set; }

		[Column("giamGiaSP")]
		[Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100%")]
		public double? GiamGiaSp { get; set; }

		[Column("anhChiTietSP")]
		[StringLength(150)]
		public string? AnhChiTietSp { get; set; }
		public string? TenMau { get; set; } // Optional: for display
		public string? TenCl { get; set; }  // Optional: for display
											// Detail image file
		public IFormFile? AnhChiTietSpFile { get; set; }
	}
}
