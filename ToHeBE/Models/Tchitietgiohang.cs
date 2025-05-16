using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToHeBE.Models;


namespace ToHeBE.Models
{
	[Table("tchitietgiohang")]
	public class Tchitietgiohang
	{
		[Key]
		[Column("maChiTietGH")]
		public int MaChiTietGH { get; set; }

		[Column("maGioHang")]
		public int MaGioHang { get; set; }

		[Column("maSanPham")]
		public int MaSanPham { get; set; }

		[Column("slSP")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0")]
		public int SlSP { get; set; }

		[Column("donGia")]
		public double? DonGia { get; set; }

		[ForeignKey(nameof(MaGioHang))]
		[InverseProperty(nameof(Tgiohang.Tchitietgiohangs))]
		public virtual Tgiohang MaGioHangNavigation { get; set; } = null!;

		[ForeignKey(nameof(MaSanPham))]
		[InverseProperty(nameof(Tsanpham.Tchitietgiohangs))]
		public virtual Tsanpham MaSanPhamNavigation { get; set; } = null!;
	}
}