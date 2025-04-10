using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("tmau")]
    public partial class Tmau
    {
        public Tmau()
        {
            TchitietSps = new HashSet<TchitietSp>();
        }

        [Key]
        [Column("maMau")]
        public int MaMau { get; set; }
        [Column("tenMau")]
        [StringLength(45)]
        public string TenMau { get; set; } = null!;

        [InverseProperty(nameof(TchitietSp.MaMauNavigation))]
        public virtual ICollection<TchitietSp> TchitietSps { get; set; }
    }
}
