using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.Auth
{
	public class DanhGiaRequest
	{
		[Required]
		public int MaSanPham { get; set; }
		[Required]
		[Range(1, 5)]
		public int DanhGia { get; set; }
		[StringLength(300)]
		public string? BinhLuan { get; set; }
	}
}
