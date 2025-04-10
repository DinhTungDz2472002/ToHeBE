using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tchitietSP")]
    public partial class TchitietSp
    {
        public TchitietSp()
        {
            Tanhctsps = new HashSet<Tanhctsp>();
        }

        [Key]
        [Column("maChiTietSP")]
        public int MaChiTietSp { get; set; }
        [Column("maSanPham")]
        public int MaSanPham { get; set; }
        [Column("maMau")]
        public int MaMau { get; set; }
        [Column("maCL")]
        public int MaCl { get; set; }
        [Column("giamGiaSP")]
        public double? GiamGiaSp { get; set; }
        [Column("anhChiTietSP")]
        [StringLength(150)]
        public string? AnhChiTietSp { get; set; }

        [ForeignKey(nameof(MaCl))]
        [InverseProperty(nameof(Tchatlieu.TchitietSps))]
        public virtual Tchatlieu MaClNavigation { get; set; } = null!;
        [ForeignKey(nameof(MaMau))]
        [InverseProperty(nameof(Tmau.TchitietSps))]
        public virtual Tmau MaMauNavigation { get; set; } = null!;
        [ForeignKey(nameof(MaSanPham))]
        [InverseProperty(nameof(Tsanpham.TchitietSps))]
        public virtual Tsanpham MaSanPhamNavigation { get; set; } = null!;
        [InverseProperty(nameof(Tanhctsp.MaChiTietSpNavigation))]
        public virtual ICollection<Tanhctsp> Tanhctsps { get; set; }
    }
}
