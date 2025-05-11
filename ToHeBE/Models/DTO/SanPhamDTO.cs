using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class SanPhamDTO
	{
		[Key]
		[Column("maSanPham")]
		public int MaSanPham { get; set; }
		[Column("tenSanPham")]
		[StringLength(200)]
		public string TenSanPham { get; set; } = null!;
		[Column("giaSanPham")]
		public double GiaSanPham { get; set; }
		[Column("maLoai")]
		public int MaLoai { get; set; }
		[Column("sLTonKho")]
		public int SLtonKho { get; set; }
		[Column("anhSP")]
		[StringLength(150)]
		public string? AnhSp { get; set; }
		[Column("moTaSP")]
		[StringLength(100)]
		public string? MoTaSp { get; set; }
		[Column("ngayThemSP", TypeName = "datetime")]
		public DateTime? NgayThemSp { get; set; }
		[Column("Status")]
		public bool Status { get; set; } = true;

	}
}
