using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.Auth
{
	public class ThemSanPhamCartModel
	{
		[Required]
		public int MaSanPham { get; set; }

		[Required]
		public int SlSP { get; set; }
	}
}
