namespace LauAPI.Model
{
    public class HoaDonKhachHang
    {
        public int MaCTHD { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public int? MaMonAn { get; set; }
        public string? TenMon { get; set; }
        public int? MaBan { get; set; }
        public int MaCTBH { get; set; }
        public int SoLuong { get; set; }
        public decimal PhanTramGiamGia { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
