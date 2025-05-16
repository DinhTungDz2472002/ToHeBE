using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToHeBE.Models.Auth
{
	public class RegisterModel
	{
		[Required]
		public string TenKhachHang { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string Sdt { get; set; }

		[Required]
		public string Email { get; set; }


		[Required]
		public string Password { get; set; }

	}
}
