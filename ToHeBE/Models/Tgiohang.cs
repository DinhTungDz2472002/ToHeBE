using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToHeBE.Models;

namespace ToHeBE.Models
{
	[Table("tgiohang")]
	public class Tgiohang
	{
		public Tgiohang()
		{
			Tchitietgiohangs = new HashSet<Tchitietgiohang>();
		}

		[Key]
		[Column("maGioHang")]
		public int MaGioHang { get; set; }
		[Column("maKhachHang")]
		public int MaKhachHang { get; set; }
		[Column("ngayTao")]
		public DateTime? NgayTao { get; set; }

		[ForeignKey(nameof(MaKhachHang))]
		[InverseProperty(nameof(Tkhachhang.Tgiohangs))]
		public virtual Tkhachhang MaKhachHangNavigation { get; set; } = null!;
		[InverseProperty(nameof(Tchitietgiohang.MaGioHangNavigation))]
		public virtual ICollection<Tchitietgiohang> Tchitietgiohangs { get; set; }
	}
}