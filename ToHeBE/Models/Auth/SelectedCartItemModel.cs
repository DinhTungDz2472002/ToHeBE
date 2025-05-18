using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.Auth
{
	public class SelectedCartItemModel
	{
		[Required(ErrorMessage = "Mã chi tiết giỏ hàng là bắt buộc")]
		public int MaChiTietGH { get; set; }

		[Required(ErrorMessage = "Số lượng là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int SlSP { get; set; }
	}
}
