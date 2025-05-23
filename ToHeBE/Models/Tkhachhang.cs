using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tkhachhang")]
    [Index(nameof(Email), Name = "UQ__tkhachha__A9D10534769670FC", IsUnique = true)]
    [Index(nameof(Username), Name = "UQ__tkhachha__F3DBC572F6D44E50", IsUnique = true)]
    public partial class Tkhachhang
    {
        public Tkhachhang()
        {
            Tdanhgias = new HashSet<Tdanhgia>();
            Tgiohangs = new HashSet<Tgiohang>();
            Thdbs = new HashSet<Thdb>();
        }

        [Key]
        [Column("maKhachHang")]
        public int MaKhachHang { get; set; }
        [Column("tenKhachHang")]
        [StringLength(45)]
        public string TenKhachHang { get; set; } = null!;
        [Column("username")]
        [StringLength(45)]
        public string Username { get; set; } = null!;
        [Column("gioiTinh")]
        [StringLength(10)]
        public string? GioiTinh { get; set; }
        [Column("diaChi")]
        [StringLength(150)]
        public string? DiaChi { get; set; }
        [Column("SDT")]
        [StringLength(45)]
        public string Sdt { get; set; }

		[Column("Email")]
		[StringLength(100)]
		public string? Email { get; set; }

		[Required]
		[StringLength(256)]
		public string Password { get; set; }

		[Column("Role")]
		[StringLength(50)]
		public string Role { get; set; } = "User"; // Giá trị mặc định là "User"

		[InverseProperty(nameof(Tdanhgia.MaKhachHangNavigation))]
        public virtual ICollection<Tdanhgia> Tdanhgias { get; set; }
        [InverseProperty(nameof(Tgiohang.MaKhachHangNavigation))]
        public virtual ICollection<Tgiohang> Tgiohangs { get; set; }
        [InverseProperty(nameof(Thdb.MaKhachHangNavigation))]
        public virtual ICollection<Thdb> Thdbs { get; set; }
    }
}
