using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToHeBE.Models
{
    [Table("taikhoan")]
    [Index(nameof(Username), Name = "UQ__taikhoan__536C85E4CB3A1FAE", IsUnique = true)]
    [Index(nameof(Email), Name = "UQ__taikhoan__A9D105342598C313", IsUnique = true)]
    public partial class Taikhoan
    {
        [Key]
        [Column("idUser")]
        public int IdUser { get; set; }
        [StringLength(45)]
        public string Username { get; set; } = null!;
        [Column("firstName")]
        [StringLength(45)]
        public string FirstName { get; set; } = null!;
        [Column("lastName")]
        [StringLength(45)]
        public string LastName { get; set; } = null!;
        [StringLength(100)]
        public string Email { get; set; } = null!;
        [StringLength(45)]
        public string? Contact { get; set; }
        [StringLength(100)]
        public string? Image { get; set; }
        [StringLength(255)]
        public string Password { get; set; } = null!;
        [Column("maChucVu")]
        public int? MaChucVu { get; set; }

        [ForeignKey(nameof(MaChucVu))]
        [InverseProperty(nameof(Tchucvu.Taikhoans))]
        public virtual Tchucvu? MaChucVuNavigation { get; set; }
    }
}
