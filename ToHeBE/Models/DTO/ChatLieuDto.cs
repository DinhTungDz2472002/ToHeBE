using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class ChatLieuDto
	{
		[Column("tenCL")]
		[StringLength(45)]
		public string TenCl { get; set; } = null!;
	}
}
