using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tchatlieu")]
    public partial class Tchatlieu
    {
        public Tchatlieu()
        {
            TchitietSps = new HashSet<TchitietSp>();
        }

        [Key]
        [Column("maCL")]
        public int MaCl { get; set; }
        [Column("tenCL")]
        [StringLength(45)]
        public string TenCl { get; set; } = null!;

        [InverseProperty(nameof(TchitietSp.MaClNavigation))]
        public virtual ICollection<TchitietSp> TchitietSps { get; set; }
    }
}
