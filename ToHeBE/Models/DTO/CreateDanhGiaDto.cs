using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToHeBE.Models.DTO
{
	public class CreateDanhGiaDto
	{
		public int MaSanPham { get; set; }
		
		public int MaChiTietHdb { get; set; }

		[Range(1, 5)]
		public int DanhGia { get; set; } // Rating (e.g., 1-5)


		public string? BinhLuan { get; set; } // Comment
	}
}
