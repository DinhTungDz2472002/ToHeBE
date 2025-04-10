using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tloai")]
    public partial class Tloai
    {
        public Tloai()
        {
            Tsanphams = new HashSet<Tsanpham>();
        }

        [Key]
        [Column("maLoai")]
        public int MaLoai { get; set; }
        [Column("tenLoai")]
        [StringLength(45)]
        public string TenLoai { get; set; } = null!;
        [Column("moTa")]
        [StringLength(45)]
        public string? MoTa { get; set; }

        [InverseProperty(nameof(Tsanpham.MaLoaiNavigation))]
        public virtual ICollection<Tsanpham> Tsanphams { get; set; }
    }
}
