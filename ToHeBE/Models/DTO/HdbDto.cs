using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class HdbDto
	{
		[Column("maHDB")]
		public int MaHdb { get; set; }
		[Column("maKhachHang")]
		public int MaKhachHang { get; set; }
		[Column("ngayLapHDB", TypeName = "datetime")]
		public DateTime? NgayLapHdb { get; set; }
		[Column("giamGia")]
		public double? GiamGia { get; set; }
		[Column("PTTT")]
		[StringLength(45)]
		public string? Pttt { get; set; }
		[Column("tongTienHDB")]
		public double? TongTienHdb { get; set; }
	}
}
