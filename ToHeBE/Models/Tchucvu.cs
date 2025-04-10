using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tchucvu")]
    public partial class Tchucvu
    {
        public Tchucvu()
        {
            Taikhoans = new HashSet<Taikhoan>();
        }

        [Key]
        [Column("maChucVu")]
        public int MaChucVu { get; set; }
        [Column("tenChucVu")]
        [StringLength(45)]
        public string TenChucVu { get; set; } = null!;
        [Column("moTaChucVu")]
        [StringLength(100)]
        public string? MoTaChucVu { get; set; }

        [InverseProperty(nameof(Taikhoan.MaChucVuNavigation))]
        public virtual ICollection<Taikhoan> Taikhoans { get; set; }
    }
}
