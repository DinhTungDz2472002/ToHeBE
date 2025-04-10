using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tdanngia")]
    public partial class Tdanhgia
    {
        [Key]
        [Column("maDG")]
        public int MaDg { get; set; }
        [Column("maSanPham")]
        public int MaSanPham { get; set; }
        [Column("maKhachHang")]
        public int? MaKhachHang { get; set; }
        [Column("danhGia")]
        public int? DanhGia { get; set; }
        [Column("binhLuan")]
        [StringLength(300)]
        public string? BinhLuan { get; set; }
        [Column("ngayDanhGia", TypeName = "datetime")]
        public DateTime? NgayDanhGia { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        [InverseProperty(nameof(Tkhachhang.Tdanhgias))]
        public virtual Tkhachhang? MaKhachHangNavigation { get; set; }
        [ForeignKey(nameof(MaSanPham))]
        [InverseProperty(nameof(Tsanpham.Tdanhgias))]
        public virtual Tsanpham MaSanPhamNavigation { get; set; } = null!;
    }
}
