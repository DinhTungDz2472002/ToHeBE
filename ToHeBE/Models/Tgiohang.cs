using ToHeBE.Models;

public class Tgiohang
{
	public int MaGioHang { get; set; }
	public int MaKhachHang { get; set; }
	public DateTime NgayTao { get; set; }

	public Tkhachhang MaKhachHangNavigation { get; set; }
	public ICollection<Tchitietgiohang> Tchitietgiohangs { get; set; }
}