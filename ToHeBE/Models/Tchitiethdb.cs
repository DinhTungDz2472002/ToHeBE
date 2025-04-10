using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tchitiethdb")]
    public partial class Tchitiethdb
    {
        [Key]
        [Column("maChiTietHDB")]
        public int MaChiTietHdb { get; set; }
        [Column("maHDB")]
        public int MaHdb { get; set; }
        [Column("maSanPham")]
        public int MaSanPham { get; set; }
        [Column("SL")]
        public int Sl { get; set; }
        [Column("thanhTien")]
        public double? ThanhTien { get; set; }

        [ForeignKey(nameof(MaHdb))]
        [InverseProperty(nameof(Thdb.Tchitiethdbs))]
        public virtual Thdb MaHdbNavigation { get; set; } = null!;
        [ForeignKey(nameof(MaSanPham))]
        [InverseProperty(nameof(Tsanpham.Tchitiethdbs))]
        public virtual Tsanpham MaSanPhamNavigation { get; set; } = null!;
    }
}
