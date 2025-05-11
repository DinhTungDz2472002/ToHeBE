using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class GioHangDto
	{
		[Key]
		[Column("maGioHang")]
		public int MaGioHang { get; set; }
		[Column("maKhachHang")]
		public int MaKhachHang { get; set; }
		[Column("maSanPham")]
		public int MaSanPham { get; set; }
		[Column("slSP")]
		public int SlSp { get; set; }
		[Column("tongTienGH")]
		public double? TongTienGh { get; set; }
	}
}
