namespace LauAPI.Model
{
    public class PhanCongCaLam
    {
        public int MaPhanCong { get; set; }
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public int MaCa { get; set; }
        public string Tenca { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public DateTime NgayLam { get; set; }
    }
}
