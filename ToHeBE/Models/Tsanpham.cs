using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
	[Table("tsanpham")]
	public partial class Tsanpham
	{
		public Tsanpham()
		{
			TchitietSps = new HashSet<TchitietSp>();
			Tchitiethdbs = new HashSet<Tchitiethdb>();
			Tdanhgias = new HashSet<Tdanhgia>();
			Tchitietgiohangs = new HashSet<Tchitietgiohang>(); // Thay Tgiohangs bằng Tchitietgiohangs
		}

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

		[ForeignKey(nameof(MaLoai))]
		[InverseProperty(nameof(Tloai.Tsanphams))]
		public virtual Tloai MaLoaiNavigation { get; set; } = null!;
		[InverseProperty(nameof(TchitietSp.MaSanPhamNavigation))]
		public virtual ICollection<TchitietSp> TchitietSps { get; set; }
		[InverseProperty(nameof(Tchitiethdb.MaSanPhamNavigation))]
		public virtual ICollection<Tchitiethdb> Tchitiethdbs { get; set; }
		[InverseProperty(nameof(Tdanhgia.MaSanPhamNavigation))]
		public virtual ICollection<Tdanhgia> Tdanhgias { get; set; }
		[InverseProperty(nameof(Tchitietgiohang.MaSanPhamNavigation))]
		public virtual ICollection<Tchitietgiohang> Tchitietgiohangs { get; set; } // Thay Tgiohangs bằng Tchitietgiohangs
	}
}