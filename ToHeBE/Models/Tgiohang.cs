using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tgiohang")]
    public partial class Tgiohang
    {
        [Key]
        [Column("maGioHang")]
        public int MaGioHang { get; set; }
        [Column("maKhachHang")]
        public int MaKhachHang { get; set; }
        [Column("maSanPham")]
        public int MaSanPham { get; set; }
        [Column("slSP")]
        public int SlSp { get; set; }
        [Column("tongTienGH")]
        public double? TongTienGh { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        [InverseProperty(nameof(Tkhachhang.Tgiohangs))]
        public virtual Tkhachhang MaKhachHangNavigation { get; set; } = null!;
        [ForeignKey(nameof(MaSanPham))]
        [InverseProperty(nameof(Tsanpham.Tgiohangs))]
        public virtual Tsanpham MaSanPhamNavigation { get; set; } = null!;
    }
}
