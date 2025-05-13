using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ToHeBE.Models
{
    public partial class ToHeDbContext : DbContext
    {
        public ToHeDbContext()
        {
        }

        public ToHeDbContext(DbContextOptions<ToHeDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Taikhoan> Taikhoans { get; set; } = null!;
        public virtual DbSet<Tanhctsp> Tanhctsps { get; set; } = null!;
        public virtual DbSet<Tchatlieu> Tchatlieus { get; set; } = null!;
        public virtual DbSet<TchitietSp> TchitietSps { get; set; } = null!;
        public virtual DbSet<Tchitiethdb> Tchitiethdbs { get; set; } = null!;
        public virtual DbSet<Tchucvu> Tchucvus { get; set; } = null!;
        public virtual DbSet<Tdanhgia> Tdanhgias { get; set; } = null!;
        public virtual DbSet<Tgiohang> Tgiohangs { get; set; } = null!;
        public virtual DbSet<Tchitietgiohang> Tchitietgiohangs { get; set; } = null!;
		public virtual DbSet<Thdb> Thdbs { get; set; } = null!;
        public virtual DbSet<Tkhachhang> Tkhachhangs { get; set; } = null!;
        public virtual DbSet<Tloai> Tloais { get; set; } = null!;
        public virtual DbSet<Tmau> Tmaus { get; set; } = null!;
        public virtual DbSet<Tsanpham> Tsanphams { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Taikhoan>(entity =>
            {
                entity.HasKey(e => e.IdUser)
                    .HasName("PK__taikhoan__3717C98241EDB449");

                entity.HasOne(d => d.MaChucVuNavigation)
                    .WithMany(p => p.Taikhoans)
                    .HasForeignKey(d => d.MaChucVu)
                    .HasConstraintName("FK__taikhoan__maChuc__4F7CD00D");
            });

            modelBuilder.Entity<Tanhctsp>(entity =>
            {
                entity.HasKey(e => e.MaAnhCtsp)
                    .HasName("PK__tanhctsp__2ED5F72241D9C3FE");

                entity.HasOne(d => d.MaChiTietSpNavigation)
                    .WithMany(p => p.Tanhctsps)
                    .HasForeignKey(d => d.MaChiTietSp)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tanhctsp__maChiT__5EBF139D");
            });

            modelBuilder.Entity<Tchatlieu>(entity =>
            {
                entity.HasKey(e => e.MaCl)
                    .HasName("PK__tchatlie__7A3E0CEA5A964D2B");
            });

            modelBuilder.Entity<TchitietSp>(entity =>
            {
                entity.HasKey(e => e.MaChiTietSp)
                    .HasName("PK__tchitiet__3482CF8ABD83AF1A");

                entity.HasOne(d => d.MaClNavigation)
                    .WithMany(p => p.TchitietSps)
                    .HasForeignKey(d => d.MaCl)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tchitietSP__maCL__5BE2A6F2");

                entity.HasOne(d => d.MaMauNavigation)
                    .WithMany(p => p.TchitietSps)
                    .HasForeignKey(d => d.MaMau)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tchitietS__maMau__5AEE82B9");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.TchitietSps)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tchitietS__maSan__59FA5E80");
            });

            modelBuilder.Entity<Tchitiethdb>(entity =>
            {
                entity.HasKey(e => e.MaChiTietHdb)
                    .HasName("PK__tchitiet__91D2C7DD1FC7590B");

                entity.HasOne(d => d.MaHdbNavigation)
                    .WithMany(p => p.Tchitiethdbs)
                    .HasForeignKey(d => d.MaHdb)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tchitieth__maHDB__6E01572D");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.Tchitiethdbs)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tchitieth__maSan__6EF57B66");
            });

            modelBuilder.Entity<Tchucvu>(entity =>
            {
                entity.HasKey(e => e.MaChucVu)
                    .HasName("PK__tchucvu__6E42BCD9DDD121E7");
            });

            modelBuilder.Entity<Tdanhgia>(entity =>
            {
                entity.HasKey(e => e.MaDg)
                    .HasName("PK__tdanngia__7A3EF40E414BEEC0");

                entity.Property(e => e.NgayDanhGia).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.MaKhachHangNavigation)
                    .WithMany(p => p.Tdanhgias)
                    .HasForeignKey(d => d.MaKhachHang)
                    .HasConstraintName("FK__tdanngia__maKhac__6754599E");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.Tdanhgias)
                    .HasForeignKey(d => d.MaSanPham)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tdanngia__maSanP__66603565");
            });

			modelBuilder.Entity<Tgiohang>(entity =>
			{
				entity.HasKey(e => e.MaGioHang)
					.HasName("PK__tgiohang__2C76D2039C18825B");

				entity.Property(e => e.MaGioHang)
					.ValueGeneratedOnAdd(); // Tự động tăng

				entity.Property(e => e.MaKhachHang)
					.IsRequired(); // Bắt buộc

				entity.Property(e => e.NgayTao)
					.HasDefaultValueSql("GETDATE()"); // Mặc định GETDATE()

				entity.HasOne(d => d.MaKhachHangNavigation)
					.WithMany(p => p.Tgiohangs)
					.HasForeignKey(d => d.MaKhachHang)
					.OnDelete(DeleteBehavior.Cascade) // Sửa từ ClientSetNull thành Cascade
					.HasConstraintName("FK__tgiohang__maKhac__72C60C4A");
			});

			modelBuilder.Entity<Tchitietgiohang>(entity =>
			{
				entity.HasKey(e => e.MaChiTietGH)
					.HasName("PK__tchitietgiohang__<tên khóa chính>"); // Thay <tên khóa chính> bằng tên thực tế nếu có

				entity.Property(e => e.MaChiTietGH)
					.ValueGeneratedOnAdd(); // Tự động tăng

				entity.Property(e => e.MaGioHang)
					.IsRequired(); // Bắt buộc

				entity.Property(e => e.MaSanPham)
					.IsRequired(); // Bắt buộc

				entity.Property(e => e.SlSP)
					.IsRequired() // Bắt buộc
					.HasAnnotation("CheckConstraint", "slSP > 0"); // Ràng buộc slSP > 0

				entity.HasOne(d => d.MaGioHangNavigation)
					.WithMany(p => p.Tchitietgiohangs)
					.HasForeignKey(d => d.MaGioHang)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("FK__tchitietgiohang__maGioHang__<tên khóa ngoại>"); // Thay <tên khóa ngoại> bằng tên thực tế

				entity.HasOne(d => d.MaSanPhamNavigation)
					.WithMany(p => p.Tchitietgiohangs)
					.HasForeignKey(d => d.MaSanPham)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("FK__tchitietgiohang__maSanPham__<tên khóa ngoại>"); // Thay <tên khóa ngoại> bằng tên thực tế
			});
			modelBuilder.Entity<Thdb>(entity =>
            {
                entity.HasKey(e => e.MaHdb)
                    .HasName("PK__thdb__2CC85AC5008907A9");

                entity.Property(e => e.NgayLapHdb).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.MaKhachHangNavigation)
                    .WithMany(p => p.Thdbs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__thdb__maKhachHan__6B24EA82");
            });

            modelBuilder.Entity<Tkhachhang>(entity =>
            {
                entity.HasKey(e => e.MaKhachHang)
                    .HasName("PK__tkhachha__0CCB3D492032387D");
            });

            modelBuilder.Entity<Tloai>(entity =>
            {
                entity.HasKey(e => e.MaLoai)
                    .HasName("PK__tloai__E5A6B2285F2C7A59");
            });

            modelBuilder.Entity<Tmau>(entity =>
            {
                entity.HasKey(e => e.MaMau)
                    .HasName("PK__tmau__27572EAE7ABCEE8F");
            });

            modelBuilder.Entity<Tsanpham>(entity =>
            {
                entity.HasKey(e => e.MaSanPham)
                    .HasName("PK__tsanpham__5B439C43F230989B");

                entity.Property(e => e.NgayThemSp).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.MaLoaiNavigation)
                    .WithMany(p => p.Tsanphams)
                    .HasForeignKey(d => d.MaLoai)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__tsanpham__maLoai__571DF1D5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
