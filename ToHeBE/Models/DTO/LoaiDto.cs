using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.DTO
{
	public class LoaiDto
	{
		[Column("tenLoai")]
		[StringLength(45)]
		public string TenLoai { get; set; } = null!;
	}
}
