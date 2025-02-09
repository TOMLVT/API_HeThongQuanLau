namespace LauAPI.Model
{
    public class HoaDonBanHang
    {
        public int MaHDBH { get; set; }
        public DateTime NgayVao { get; set; }
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public int MaMonAn { get; set; }
        public string TenMon { get ; set; } 
        public decimal PhanTramGiamGia { get; set; }

        public int SoLuong {  get; set; }   
        public decimal GiaTienMonAn { get; set; }
        public decimal TongTien { get; set; } 
        public decimal TongTienBan { get; set; } 
    }
}
