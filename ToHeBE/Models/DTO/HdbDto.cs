using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static ToHeBE.Controllers.HdbController;


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
		[StringLength(45)]
		public string TenKhachHang { get; set; } = string.Empty;
		[Column("diaChi")]
		[StringLength(150)]
		public string DiaChi { get; set; } = string.Empty;

		[Column("SDT")]
		[StringLength(45)]
		public string Sdt { get; set; } = string.Empty;

		[Column("Status")]
		[StringLength(50)]
		public string Status { get; set; } = "Chờ xác nhận";
		public List<ChiTietHdbDto> ChiTietHoaDon { get; set; } = new List<ChiTietHdbDto>();
	}

	public class ChiTietHdbDto
	{
		[Column("maSanPham")]
		public int MaSanPham { get; set; }

		[Column("SL")]
		public int Sl { get; set; }


		[Column("thanhTien")]
		public double? ThanhTien { get; set; }
		public string? TenSanPham { get; set; }
		public string? AnhSp { get; set; }
		public double? GiaSanPham { get; set; }
	}
}