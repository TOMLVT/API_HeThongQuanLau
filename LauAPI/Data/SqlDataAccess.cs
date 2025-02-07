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
public class SqlDataAccess
{
    private readonly string _connectionString;

    public SqlDataAccess(IConfiguration configuration)
    {
        // Lấy chuỗi kết nối từ appsettings.json ----------------------------------------------------
        _connectionString = configuration.GetConnectionString("ApplicationDbContext");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured correctly.");
        }
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    public async Task<List<Table>> GetTablesAsync()
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

    // 🔹 Lấy danh sách nhân viên
    public async Task<List<NhanVien>> GetAllUsersAsync()
    {
        var users = new List<NhanVien>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
            SELECT NV.MaNV, NV.HoTen, NV.GioiTinh, NV.SDT, NV.CCCD, NV.Email, NV.MatKhau, 
                   NV.TongNgayCong, NV.TongLuong, NV.HinhAnh, PQ.MaPQ
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
                        MaPQ = reader.GetInt32(10),                     
                    });
                }
            }
        }
        return users;
    }
  

    // -------------------------------------------------------------------------------------------------------------------------------------------

    // 🔹 Đăng nhập
    public async Task<NhanVien> LoginAsync(string email, string matKhau)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Câu truy vấn SQL để lấy thông tin người dùng từ cơ sở dữ liệu
            var query = @"
                SELECT NV.MaNV, NV.HoTen, NV.Email, NV.MatKhau, NV.MaPQ
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
                        // Lấy mật khẩu đã mã hóa từ cơ sở dữ liệu
                        string hashedPasswordFromDb = reader.GetString(3);

                        // Mã hóa mật khẩu từ đầu vào và so sánh với mật khẩu trong cơ sở dữ liệu
                        string inputHashedPassword = HashPassword(matKhau);

                        if (hashedPasswordFromDb == inputHashedPassword)
                        {
                            // Nếu mật khẩu đúng, trả về đối tượng NhanVien
                            return new NhanVien
                            {
                                MaNV = reader.GetInt32(0),
                                HoTen = reader.GetString(1),
                                Email = reader.GetString(2),
                                MaPQ = reader.GetInt32(4),
                            };
                        }
                    }
                }
            }
        }
        return null; // Trả về null nếu không tìm thấy người dùng hoặc mật khẩu sai
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------

    // 🔹 Đăng ký tài khoản
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
            INSERT INTO NhanVien (HoTen, Email, SDT, MatKhau, MaPQ, TongLuong)
            VALUES (@HoTen, @Email, @SDT, @MatKhau, @MaPQ, 0)";

            using (var command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@HoTen", newUser.HoTen);
                command.Parameters.AddWithValue("@Email", newUser.Email);
                command.Parameters.AddWithValue("@SDT", newUser.SDT);
                command.Parameters.AddWithValue("@MatKhau", hashedPassword);
                command.Parameters.AddWithValue("@MaPQ", newUser.MaPQ);

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
}



// -------------------------------------------------------------------------------------------------------------------------------------------


