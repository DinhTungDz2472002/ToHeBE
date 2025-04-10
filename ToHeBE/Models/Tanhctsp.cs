using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tanhctsp")]
    public partial class Tanhctsp
    {
        [Key]
        [Column("maAnhCTSP")]
        public int MaAnhCtsp { get; set; }
        [Column("maChiTietSP")]
        public int MaChiTietSp { get; set; }
        [Column("imageCTSP")]
        [StringLength(300)]
        public string? ImageCtsp { get; set; }

        [ForeignKey(nameof(MaChiTietSp))]
        [InverseProperty(nameof(TchitietSp.Tanhctsps))]
        public virtual TchitietSp MaChiTietSpNavigation { get; set; } = null!;
    }
}
