using ToHeBE.Models;

public class Tchitietgiohang
{
	public int MaChiTietGH { get; set; }
	public int MaGioHang { get; set; }
	public int MaSanPham { get; set; }
	public int SlSP { get; set; }
	public double? DonGia { get; set; }

	public Tgiohang MaGioHangNavigation { get; set; }
	public Tsanpham MaSanPhamNavigation { get; set; }
}