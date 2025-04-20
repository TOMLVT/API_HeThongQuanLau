using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Security.Cryptography;
using System.Text;
using LauAPI.Model;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;


public class SqlDataAccess
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public SqlDataAccess(IConfiguration configuration)
    {
        // Lấy chuỗi kết nối từ appsettings.json ----------------------------------------------------
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("ApplicationDbContext");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured correctly.");
        }
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    // LẤY DANH SÁCH BÀN THEO KHU VỰC 
    public async Task<List<Table>> GetTablesAsync(int maBan)
    {
        var tables = new List<Table>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT MaBan, TenBan, TrangThai, TenKV FROM Ban B JOIN KhuVucQuan KV ON B.MaKV = KV.MaKV";
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var table = new Table
                        {
                            MaBan = reader.GetInt32(0),
                            TenBan = reader.GetString(1),
                            TrangThai = reader.GetString(2),
                            TenKV = reader.GetString(3)
                        };

                        tables.Add(table);
                    }
                }
            }
        }

        return tables;
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------

    // LẤY DANH SÁCH NHÂN VIÊN 
    public async Task<List<NhanVien>> GetAllUsersAsync() // Lưu ý truy vấn đọc theo thứ tự để reander bên dưới trong câu truy vấn ===========
    {
        var users = new List<NhanVien>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT NV.MaNV, NV.HoTen, NV.GioiTinh, NV.SDT, NV.CCCD, NV.Email, NV.MatKhau, 
                       NV.TongNgayCong, NV.TongLuong, NV.HinhAnh, NV.DiaChi, PQ.MaPQ
                FROM NhanVien NV 
                JOIN PhanQuyen PQ ON NV.MaPQ = PQ.MaPQ";

            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(new NhanVien
                    {
                        MaNV = reader.GetInt32(0),
                        HoTen = reader.GetString(1),
                        GioiTinh = reader.IsDBNull(2) ? null : reader.GetString(2),
                        SDT = reader.GetString(3),
                        CCCD = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Email = reader.GetString(5),
                        MatKhau = reader.GetString(6),
                        TongNgayCong = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                        TongLuong = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                        HinhAnh = reader.IsDBNull(9) ? null : reader.GetString(9),
                        DiaChi = reader.IsDBNull(10) ? null : reader.GetString(10),
                        MaPQ = reader.GetInt32(11),
                    });

                }
            }
        }
        return users;
    }



    // -------------------------------------------------------------------------------------------------------------------------------------------

    // THÔNG TIN ĐĂNG NHẬP
    public async Task<NhanVien> LoginAsync(string email, string matKhau)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
            SELECT NV.MaNV, NV.HoTen, NV.GioiTinh, NV.SDT, NV.CCCD, NV.Email, NV.MatKhau, 
                   NV.TongNgayCong, NV.TongLuong, NV.HinhAnh, PQ.MaPQ
            FROM NhanVien NV 
            JOIN PhanQuyen PQ ON NV.MaPQ = PQ.MaPQ
            WHERE NV.Email = @Email";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string hashedPasswordFromDb = reader.GetString(6); // Mật khẩu đã băm trong DB

                        // Băm mật khẩu nhập vào bằng SHA-256
                        string hashedInputPassword = HashPassword(matKhau);

                        if (hashedInputPassword == hashedPasswordFromDb)
                        {
                            return new NhanVien
                            {
                                MaNV = reader.GetInt32(0),
                                HoTen = reader.GetString(1),
                                GioiTinh = reader.IsDBNull(2) ? null : reader.GetString(2),
                                SDT = reader.GetString(3),
                                CCCD = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.GetString(5),
                                MatKhau = hashedPasswordFromDb,
                                TongNgayCong = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                TongLuong = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                HinhAnh = reader.IsDBNull(9) ? null : reader.GetString(9),
                                MaPQ = reader.GetInt32(10),
                            };
                        }
                    }
                }
            }
        }
        return null; // Đăng nhập thất bại
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------

    // THÔNG TIN ĐĂNG KÝ TÀI KHOẢN CÓ MÃ HÓA 
    public async Task<bool> RegisterAsync(NhanVien newUser)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Kiểm tra email đã tồn tại
            var checkQuery = "SELECT COUNT(*) FROM NhanVien WHERE Email = @Email";
            using (var checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@Email", newUser.Email);
                int count = (int)await checkCommand.ExecuteScalarAsync();
                if (count > 0) return false; // Email đã tồn tại
            }

            // Mã hóa mật khẩu bằng SHA-256 (không sử dụng salt)
            string hashedPassword = HashPassword(newUser.MatKhau);

            // Thêm nhân viên mới vào database
            var insertQuery = @"
        INSERT INTO NhanVien (HoTen, Email, SDT, MatKhau, MaPQ, TongLuong, GioiTinh, CCCD, HinhAnh)
        VALUES (@HoTen, @Email, @SDT, @MatKhau, @MaPQ, 0, @GioiTinh, @CCCD, @HinhAnh)";

            using (var command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@HoTen", newUser.HoTen);
                command.Parameters.AddWithValue("@Email", newUser.Email);
                command.Parameters.AddWithValue("@SDT", newUser.SDT);
                command.Parameters.AddWithValue("@MatKhau", hashedPassword);
                command.Parameters.AddWithValue("@MaPQ", newUser.MaPQ);
                command.Parameters.AddWithValue("@GioiTinh", newUser.GioiTinh ?? (object)DBNull.Value); // Nếu không có giá trị, set là null
                command.Parameters.AddWithValue("@CCCD", newUser.CCCD ?? (object)DBNull.Value); // Nếu không có giá trị, set là null
                command.Parameters.AddWithValue("@HinhAnh", newUser.HinhAnh ?? (object)DBNull.Value); // Nếu không có giá trị, set là null

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }


    // -------------------------------------------------------------------------------------------------------------------------------------------

    // Hàm băm mật khẩu với SHA-256
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // Chuyển byte sang dạng hex
            }
            return builder.ToString();
        }
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THÔNG TIN NHÓM MÓN ĂN 
    public async Task<List<NhomMonAn>> GetDishGroupAsync()
    {
        var dishGroup = new List<NhomMonAn>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT * FROM NhomMonAn";
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var nhomMonAn = new NhomMonAn
                        {
                            MaNhomMonAn = reader.GetInt32(0),
                            TenNhom = reader.GetString(1),
                            HinhAnh = reader.IsDBNull(2) ? null : reader.GetString(2),
                        };

                        dishGroup.Add(nhomMonAn);
                    }
                }
            }
        }

        return dishGroup;
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------
    // LẤY THÔNG TIN MÓN ĂN 
    public async Task<List<MonAn>> GetDishesAsync(int maMonAn)
    {
        var dishes = new List<MonAn>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT 
                ma.MaMonAn, 
                ma.TenMon, 
                ma.HinhAnh, 
                ma.DonViTinh, 
                ma.GiaTien, 
                ma.GiaSauGiam,
                ma.MaNhomMonAn, 
                nma.TenNhom,
                ma.MaQRMonAn,
                ma.MoTaMonAn,
                ma.SoLuongConLai,
                ma.SoLuotDaBan,
                ma.GiamGia
                FROM MonAn ma
                LEFT JOIN NhomMonAn nma ON ma.MaNhomMonAn = nma.MaNhomMonAn";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var monAn = new MonAn
                        {
                            MaMonAn = reader.GetInt32(0),
                            TenMon = reader.GetString(1),
                            HinhAnh = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DonViTinh = reader.IsDBNull(3) ? null : reader.GetString(3),
                            GiaTien = reader.GetDecimal(4),
                            GiaSauGiam = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                            MaNhomMonAn = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                            TenNhom = reader.IsDBNull(7) ? null : reader.GetString(7),
                            MaQRMonAn = reader.IsDBNull(8) ? null : reader.GetString(8),
                            MoTaMonAn = reader.IsDBNull(9) ? null : reader.GetString(9),
                            SoLuongConLai = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                            SoLuotDaBan = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                            GiamGia = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12)
                        };



                        dishes.Add(monAn);
                    }
                }
            }
        }

        return dishes;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THÊM MÓN ĂN VÀO HÓA ĐƠN CHO BÀN
    public async Task<string> AddDishToOrderAsync(OrderRequest orderRequest)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // Mở kết nối
            await connection.OpenAsync();

            // Bắt đầu transaction
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Kiểm tra nếu nhân viên và số lượng hợp lệ
                    if (orderRequest.MaNV <= 0)
                    {
                        return "Vui lòng chọn nhân viên.";
                    }

                    if (orderRequest.SoLuong <= 0)
                    {
                        return "Vui lòng nhập số lượng hợp lệ.";
                    }

                    // Kiểm tra số lượng món ăn trong kho
                    string checkStockQuery = "SELECT SoLuong FROM SuDungNguyenLieu WHERE MaMonAn = @MaMon";
                    using (var checkStockCmd = new SqlCommand(checkStockQuery, connection, transaction))
                    {
                        checkStockCmd.Parameters.AddWithValue("@MaMon", orderRequest.MaMonAn);
                        object stockResult = await checkStockCmd.ExecuteScalarAsync();

                        if (stockResult == null || stockResult == DBNull.Value)
                        {
                            return "Món ăn không tồn tại trong kho.";
                        }

                        int currentStock = Convert.ToInt32(stockResult);

                        if (currentStock < orderRequest.SoLuong)
                        {
                            return "Món ăn này không đủ số lượng.";
                        }
                    }

                    // Kiểm tra xem món ăn đã tồn tại trong hóa đơn của bàn chưa
                    string checkQuery = "SELECT COUNT(*) FROM HoaDonBanHang WHERE MaBan = @MaBan AND MaMonAn = @MaMon";
                    using (var checkCmd = new SqlCommand(checkQuery, connection, transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@MaBan", orderRequest.MaBan);
                        checkCmd.Parameters.AddWithValue("@MaMon", orderRequest.MaMonAn);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();

                        if (exists > 0)
                        {
                            // Cập nhật số lượng món ăn trong hóa đơn nếu món đã có trong hóa đơn
                            string updateQuery = @"UPDATE HoaDonBanHang 
                                            SET SoLuong = SoLuong + @SoLuong 
                                            WHERE MaBan = @MaBan AND MaMonAn = @MaMon";
                            using (var updateCmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@MaBan", orderRequest.MaBan);
                                updateCmd.Parameters.AddWithValue("@MaMon", orderRequest.MaMonAn);
                                updateCmd.Parameters.AddWithValue("@SoLuong", orderRequest.SoLuong);
                                await updateCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Thêm món ăn mới vào hóa đơn
                            string insertQuery = @"INSERT INTO HoaDonBanHang (MaBan, MaMonAn, SoLuong, MaNV, NgayVao) 
                                            VALUES (@MaBan, @MaMon, @SoLuong, @MaNV, @NgayVao)";
                            using (var insertCmd = new SqlCommand(insertQuery, connection, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@MaBan", orderRequest.MaBan);
                                insertCmd.Parameters.AddWithValue("@MaMon", orderRequest.MaMonAn);
                                insertCmd.Parameters.AddWithValue("@SoLuong", orderRequest.SoLuong);
                                insertCmd.Parameters.AddWithValue("@MaNV", orderRequest.MaNV);
                                insertCmd.Parameters.AddWithValue("@NgayVao", DateTime.Now);
                                await insertCmd.ExecuteNonQueryAsync();
                            }
                        }

                        // Cập nhật lại số lượng nguyên liệu trong kho
                        string updateStockQuery = "UPDATE SuDungNguyenLieu SET SoLuong = SoLuong - @SoLuong WHERE MaMonAn = @MaMon";
                        using (var updateStockCmd = new SqlCommand(updateStockQuery, connection, transaction))
                        {
                            updateStockCmd.Parameters.AddWithValue("@SoLuong", orderRequest.SoLuong);
                            updateStockCmd.Parameters.AddWithValue("@MaMon", orderRequest.MaMonAn);
                            await updateStockCmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Commit giao dịch
                    transaction.Commit();
                    return "Món ăn đã được thêm vào hóa đơn.";
                }
                catch (Exception ex)
                {
                    // Rollback giao dịch nếu có lỗi
                    transaction.Rollback();
                    return $"Lỗi khi lưu món: {ex.Message}";
                }
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------

    // CẬP NHẬT TRẠNG THÁI CỦA BÀN KHI THÊM MÓN 
    public async Task<bool> UpdateTableStatusAsync(int maBan, string newStatus)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = "UPDATE Ban SET TrangThai = @TrangThai WHERE MaBan = @MaBan";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TrangThai", newStatus);
                command.Parameters.AddWithValue("@MaBan", maBan);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;  // Trả về true nếu cập nhật thành công, false nếu không thay đổi được
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------

    //LẤY THÔNG TIN TỔNG TIỀN + MÓN ĂN TRONG BÀN TỪ BẢNG -> HÓA ĐƠN BÁN HÀNG
    public async Task<List<HoaDonBanHang>> GetHoaDonBanHangAsync(int maBan)
    {
        var hoaDonList = new List<HoaDonBanHang>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
        SELECT 
            hdbh.MaHDBH, 
            hdbh.NgayVao, 
            hdbh.MaNV, 
            nv.HoTen,
            hdbh.MaBan, 
            b.TenBan,
            hdbh.MaMonAn, 
            hdbh.PhanTramGiamGia,
            m.TenMon,
            m.GiaTien,
            hdbh.SoLuong
        FROM HoaDonBanHang hdbh
        JOIN MonAn m ON hdbh.MaMonAn = m.MaMonAn
        JOIN NhanVien nv ON hdbh.MaNV = nv.MaNV
        JOIN ban b ON hdbh.MaBan = b.MaBan
        WHERE hdbh.MaBan = @MaBan";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MaBan", maBan);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var hoaDon = new HoaDonBanHang
                        {
                            MaHDBH = reader.GetInt32(0),
                            NgayVao = reader.GetDateTime(1),
                            MaNV = reader.GetInt32(2),
                            HoTen = reader.GetString(3),
                            MaBan = reader.GetInt32(4),
                            TenBan = reader.GetString(5),
                            MaMonAn = reader.GetInt32(6),
                            PhanTramGiamGia = reader.GetDecimal(7),
                            TenMon = reader.GetString(8),
                            GiaTienMonAn = reader.GetDecimal(9),
                            SoLuong = reader.GetInt32(10), // Gán số lượng
                        };

                        // Tính tổng tiền cho mỗi món ăn của hóa đơn
                        hoaDon.TongTien = hoaDon.GiaTienMonAn * hoaDon.SoLuong * (1 - hoaDon.PhanTramGiamGia / 100);

                        hoaDonList.Add(hoaDon);
                    }
                }
            }
        }

        // Tính tổng tiền của tất cả các món ăn trong bàn đó
        decimal tongTienBan = hoaDonList.Sum(hd => hd.TongTien);

        // Lưu tổng tiền của bàn vào mỗi hóa đơn
        foreach (var hoaDon in hoaDonList)
        {
            hoaDon.TongTienBan = tongTienBan;
        }

        return hoaDonList;
    }


    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THANH TOÁN BÀN ĂN 
    public async Task<decimal> ThanhToanAsync(ThanhToan request)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    decimal totalAmount = 0;
                    int totalQuantity = 0;
                    decimal discountPercent = request.PhanTramGiamGia;

                    // Lấy chi tiết món ăn từ bàn
                    string getDetailsQuery = @"
                    SELECT h.MaMonAn, h.SoLuong, s.GiaTien, s.TenMon, (h.SoLuong * s.GiaTien) AS ThanhTien
                    FROM HoaDonBanHang h
                    JOIN MonAn s ON h.MaMonAn = s.MaMonAn
                    WHERE h.MaBan = @MaBan";

                    SqlCommand cmdDetails = new SqlCommand(getDetailsQuery, conn, transaction);
                    cmdDetails.Parameters.AddWithValue("@MaBan", request.MaBan);

                    SqlDataReader reader = await cmdDetails.ExecuteReaderAsync();

                    List<string> monAnList = new List<string>();
                    List<int> maMonAnList = new List<int>(); // Lưu MaMonAn của món ăn
                    while (await reader.ReadAsync())
                    {
                        monAnList.Add(reader["TenMon"].ToString());
                        maMonAnList.Add(Convert.ToInt32(reader["MaMonAn"])); // Lưu MaMonAn
                        totalAmount += Convert.ToDecimal(reader["ThanhTien"]);
                        totalQuantity += Convert.ToInt32(reader["SoLuong"]);
                    }
                    reader.Close();

                    // Tính toán giảm giá
                    decimal discountAmount = (discountPercent / 100) * totalAmount;
                    decimal finalAmount = totalAmount - discountAmount;

                    // Thêm vào bảng ChiTietBanHang
                    foreach (var maMonAn in maMonAnList) // Duyệt qua từng món ăn
                    {
                        string insertChiTietBanHangQuery = @"
                        INSERT INTO ChiTietBanHang (SoLuong, MaMonAn)
                        OUTPUT INSERTED.MaCTBH
                        VALUES (@SoLuong, @MaMonAn)";

                        SqlCommand cmdInsertChiTiet = new SqlCommand(insertChiTietBanHangQuery, conn, transaction);
                        cmdInsertChiTiet.Parameters.AddWithValue("@SoLuong", totalQuantity);
                        cmdInsertChiTiet.Parameters.AddWithValue("@MaMonAn", maMonAn); // Sử dụng MaMonAn từ danh sách
                        int maCTBH = (int)await cmdInsertChiTiet.ExecuteScalarAsync();

                        // Thêm vào bảng HoaDonKhachHang
                        string insertHoaDonKhachHangQuery = @"
                        INSERT INTO HoaDonKhachHang (MaMonAn, TenMon, NgayThanhToan, MaCTBH, SoLuong, PhanTramGiamGia, DonGia, ThanhTien, MaBan)
                        VALUES (@MaMonAn, @TenMon, GETDATE(), @MaCTBH, @SoLuong, @PhanTramGiamGia, @DonGia, @ThanhTien, @MaBan)";

                        SqlCommand cmdInsertHoaDon = new SqlCommand(insertHoaDonKhachHangQuery, conn, transaction);
                        cmdInsertHoaDon.Parameters.AddWithValue("@MaMonAn", maMonAn); // Sử dụng MaMonAn
                        cmdInsertHoaDon.Parameters.AddWithValue("@TenMon", string.Join(",", monAnList));
                        cmdInsertHoaDon.Parameters.AddWithValue("@MaBan", request.MaBan);
                        cmdInsertHoaDon.Parameters.AddWithValue("@MaCTBH", maCTBH);
                        cmdInsertHoaDon.Parameters.AddWithValue("@SoLuong", totalQuantity);
                        cmdInsertHoaDon.Parameters.AddWithValue("@PhanTramGiamGia", discountPercent);
                        cmdInsertHoaDon.Parameters.AddWithValue("@DonGia", 0); // Giá cần được lấy từ cơ sở dữ liệu
                        cmdInsertHoaDon.Parameters.AddWithValue("@ThanhTien", finalAmount);

                        await cmdInsertHoaDon.ExecuteNonQueryAsync();
                    }

                    // Xóa các món ăn trong HoaDonBanHang của bàn này
                    string deleteHoaDonBanHangQuery = @" DELETE FROM HoaDonBanHang WHERE MaBan = @MaBan";
                    SqlCommand cmdDeleteHoaDon = new SqlCommand(deleteHoaDonBanHangQuery, conn, transaction);
                    cmdDeleteHoaDon.Parameters.AddWithValue("@MaBan", request.MaBan);
                    cmdDeleteHoaDon.ExecuteNonQuery();

                    // Cập nhật trạng thái bàn
                    string updateTableStatusQuery = "UPDATE Ban SET TrangThai = N'Trống' WHERE MaBan = @MaBan";
                    SqlCommand cmdUpdateTableStatus = new SqlCommand(updateTableStatusQuery, conn, transaction);
                    cmdUpdateTableStatus.Parameters.AddWithValue("@MaBan", request.MaBan);
                    await cmdUpdateTableStatus.ExecuteNonQueryAsync();

                    // Commit giao dịch
                    transaction.Commit();
                    return finalAmount;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------
    // CODE XÓA MÓN ĂN TRONG BÀN 

    public async Task<bool> RemoveDishFromOrderAsync(int maBan, int maMonAn , int SoLuongXoa)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Kiểm tra xem món ăn có tồn tại không
                    string checkQuery = "SELECT SoLuong FROM HoaDonBanHang WHERE MaBan = @MaBan AND MaMonAn = @MaMonAn";
                    using (var checkCmd = new SqlCommand(checkQuery, connection, transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@MaBan", maBan);
                        checkCmd.Parameters.AddWithValue("@MaMonAn", maMonAn);

                        object result = await checkCmd.ExecuteScalarAsync();
                        if (result == null)
                        {
                            transaction.Rollback();
                            return false; // Món ăn không tồn tại trong hóa đơn
                        }

                        int currentQuantity = Convert.ToInt32(result);

                        if (SoLuongXoa >= currentQuantity)
                        {
                            string deleteQuery = "DELETE FROM HoaDonBanHang WHERE MaBan = @MaBan AND MaMonAn = @MaMonAn";
                            using (var deleteCmd = new SqlCommand(deleteQuery, connection, transaction))
                            {
                                deleteCmd.Parameters.AddWithValue("@MaBan", maBan);
                                deleteCmd.Parameters.AddWithValue("@MaMonAn", maMonAn);
                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Nếu số lượng cần xóa nhỏ hơn số lượng hiện có, chỉ giảm số lượng
                            string updateQuery = "UPDATE HoaDonBanHang SET SoLuong = SoLuong - @SoLuongXoa WHERE MaBan = @MaBan AND MaMonAn = @MaMonAn";
                            using (var updateCmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@MaBan", maBan);
                                updateCmd.Parameters.AddWithValue("@MaMonAn", maMonAn);
                                updateCmd.Parameters.AddWithValue("@SoLuongXoa", SoLuongXoa);
                                await updateCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // Kiểm tra xem bàn còn món nào không
                    string countQuery = "SELECT COUNT(*) FROM HoaDonBanHang WHERE MaBan = @MaBan";
                    using (var countCmd = new SqlCommand(countQuery, connection, transaction))
                    {
                        countCmd.Parameters.AddWithValue("@MaBan", maBan);
                        int count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                        if (count == 0)
                        {
                            // Nếu bàn không còn món nào, cập nhật trạng thái về "Trống"
                            string updateStatusQuery = "UPDATE Ban SET TrangThai = N'Trống' WHERE MaBan = @MaBan";
                            using (var updateStatusCmd = new SqlCommand(updateStatusQuery, connection, transaction))
                            {
                                updateStatusCmd.Parameters.AddWithValue("@MaBan", maBan);
                                await updateStatusCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THÔNG TIN PHÂN CÔNG CA LÀM 
    public async Task<List<PhanCongCaLam>> GetworkAsync()
    {
        var works = new List<PhanCongCaLam>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    pc.MaPhanCong,
                    pc.MaNV,
                    nv.HoTen,
                    pc.MaCa,
                    cl.TenCa,
                    cl.GioBatDau,
                    cl.GioKetThuc,
                    pc.NgayLam
                FROM PhanCongCaLam pc
                JOIN CaLam cl ON pc.MaCa = cl.MaCa
                JOIN NhanVien nv ON pc.MaNV = nv.MaNV";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var work = new PhanCongCaLam
                        {
                            MaPhanCong = reader.GetInt32(0),
                            MaNV = reader.GetInt32(1),
                            HoTen = reader.GetString(2),
                            MaCa = reader.GetInt32(3),
                            Tenca = reader.GetString(4),
                            GioBatDau = reader.GetTimeSpan(5),
                            GioKetThuc = reader.GetTimeSpan(6),
                            NgayLam = reader.GetDateTime(7)
                        };

                        works.Add(work);
                    }
                }
            }
        }

        return works;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THÔNG TIN CHẤM CÔNG NHÂN VIÊN 
    public async Task ChamCongNhanVienAsync(ChamCongNhanVien chamCong)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                INSERT INTO ChamCongNhanVien (NgayCong, TrangThai, MaNV)
                VALUES (@NgayCong, @TrangThai, @MaNV)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NgayCong", chamCong.NgayCong);
                command.Parameters.AddWithValue("@TrangThai", chamCong.TrangThai);
                command.Parameters.AddWithValue("@MaNV", chamCong.MaNV);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------
    // NGUYÊN LIỆU THÊM LẨU 
    public async Task<List<NguyenLieuThemLau>> GetNguyenLieuAsync()
    {
        var nguyenLieuList = new List<NguyenLieuThemLau>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT MaNL, TenNguyenLieu, DonViTinh, DonGia, SoLuong
                FROM NguyenLieuThemLau";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var nguyenLieu = new NguyenLieuThemLau
                        {
                            MaNL = reader.GetInt32(0),
                            TenNguyenLieu = reader.GetString(1),
                            DonViTinh = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DonGia = reader.GetDecimal(3),
                            SoLuong = reader.GetInt32(4)
                        };

                        nguyenLieuList.Add(nguyenLieu);
                    }
                }
            }
        }

        return nguyenLieuList;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    // THÔNG TIN SỬ DỤNG NGUYÊN LIỆU
    public async Task<List<SuDungNguyenLieu>> GetSuDungNguyenLieuAsync()
    {
        var suDungList = new List<SuDungNguyenLieu>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    sdnl.MaSuDung, 
                    sdnl.SoLuong, 
                    sdnl.NgayXuat, 
                    sdnl.MaMonAn, 
                    ma.TenMon,
                    sdnl.MaNL, 
                    nl.TenNguyenLieu
                FROM SuDungNguyenLieu sdnl
                JOIN MonAn ma ON sdnl.MaMonAn = ma.MaMonAn
                JOIN NguyenLieuThemLau nl ON sdnl.MaNL = nl.MaNL";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var suDung = new SuDungNguyenLieu
                        {
                            MaSuDung = reader.GetInt32(0),
                            SoLuong = reader.GetInt32(1),
                            NgayXuat = reader.GetDateTime(2),
                            MaMonAn = reader.GetInt32(3),
                            TenMonAn = reader.GetString(4),
                            MaNL = reader.GetInt32(5),
                            TenNguyenLieu = reader.GetString(6)
                        };

                        suDungList.Add(suDung);
                    }
                }
            }
        }

        return suDungList;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    //LẤY THÔNG TIN HÓA ĐƠN KHÁCH HÀNG
    public async Task<List<HoaDonKhachHang>> GetHoaDonKhachHangAsync()
    {
        var hoaDonList = new List<HoaDonKhachHang>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    MaCTHD, 
                    NgayThanhToan, 
                    MaMonAn, 
                    TenMon, 
                    MaBan, 
                    MaCTBH, 
                    SoLuong, 
                    PhanTramGiamGia, 
                    DonGia, 
                    ThanhTien
                FROM HoaDonKhachHang";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var hoaDon = new HoaDonKhachHang
                        {
                            MaCTHD = reader.GetInt32(0),
                            NgayThanhToan = reader.GetDateTime(1),
                            MaMonAn = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                            TenMon = reader.IsDBNull(3) ? null : reader.GetString(3),
                            MaBan = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                            MaCTBH = reader.GetInt32(5),
                            SoLuong = reader.GetInt32(6),
                            PhanTramGiamGia = reader.GetDecimal(7),
                            DonGia = reader.GetDecimal(8),
                            ThanhTien = reader.GetDecimal(9)
                        };

                        hoaDonList.Add(hoaDon);
                    }
                }
            }
        }

        return hoaDonList;
    }
    // -------------------------------------------------------------------------------------------------------------------------------------------
   
}



