using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("thdb")]
    public partial class Thdb
    {
        public Thdb()
        {
            Tchitiethdbs = new HashSet<Tchitiethdb>();
        }

        [Key]
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
		[Column("tenKhachHang")]
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


		[ForeignKey(nameof(MaKhachHang))]
        [InverseProperty(nameof(Tkhachhang.Thdbs))]
        public virtual Tkhachhang MaKhachHangNavigation { get; set; } = null!;
        [InverseProperty(nameof(Tchitiethdb.MaHdbNavigation))]
        public virtual ICollection<Tchitiethdb> Tchitiethdbs { get; set; }
    }
}
