using static ToHeBE.Controllers.OrderController;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.Auth
{
	// Model để nhận dữ liệu từ client
	public class TaoHoaDonModel
	{
		[Required(ErrorMessage = "Họ Tên là bắt buộc")]
		public string TenKhachHang { get; set; }

		[Required(ErrorMessage = "Địa chỉ là bắt buộc")]
		public string DiaChi { get; set; }

		[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string SDT { get; set; }

		[Required(ErrorMessage = "Danh sách sản phẩm được chọn là bắt buộc")]
		public List<SelectedCartItemModel> SelectedItems { get; set; }
	}
}
