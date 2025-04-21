using Microsoft.Extensions.Configuration;
using LauAPI.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

public class SqlMonAnAccess
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public SqlMonAnAccess(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("ApplicationDbContext");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured correctly.");
        }
    }

    public async Task<bool> AddMonAnAsync(MonAn model)
    {
        var query = @"INSERT INTO MonAn 
                    (TenMon, HinhAnh, DonViTinh, GiaTien, GiaSauGiam, MaNhomMonAn, MaQRMonAn, MoTaMonAn, SoLuongConLai, SoLuotDaBan, GiamGia)
                    VALUES 
                    (@TenMon, @HinhAnh, @DonViTinh, @GiaTien, @GiaSauGiam, @MaNhomMonAn, @MaQRMonAn, @MoTaMonAn, @SoLuongConLai, @SoLuotDaBan, @GiamGia)";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@TenMon", model.TenMon ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DonViTinh", model.DonViTinh ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GiaTien", model.GiaTien);
            command.Parameters.AddWithValue("@GiaSauGiam", model.GiaSauGiam ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaNhomMonAn", model.MaNhomMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaQRMonAn", model.MaQRMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MoTaMonAn", model.MoTaMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SoLuongConLai", model.SoLuongConLai ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SoLuotDaBan", model.SoLuotDaBan ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GiamGia", model.GiamGia ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }

    public async Task<bool> UpdateMonAnAsync(MonAn model)
    {
        var query = @"UPDATE MonAn SET 
                        TenMon = @TenMon,
                        HinhAnh = @HinhAnh,
                        DonViTinh = @DonViTinh,
                        GiaTien = @GiaTien,
                        GiaSauGiam = @GiaSauGiam,
                        MaNhomMonAn = @MaNhomMonAn,
                        MaQRMonAn = @MaQRMonAn,
                        MoTaMonAn = @MoTaMonAn,
                        SoLuongConLai = @SoLuongConLai,
                        SoLuotDaBan = @SoLuotDaBan,
                        GiamGia = @GiamGia
                    WHERE MaMonAn = @Id";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@TenMon", model.TenMon ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DonViTinh", model.DonViTinh ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GiaTien", model.GiaTien);
            command.Parameters.AddWithValue("@GiaSauGiam", model.GiaSauGiam ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaNhomMonAn", model.MaNhomMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaQRMonAn", model.MaQRMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MoTaMonAn", model.MoTaMonAn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SoLuongConLai", model.SoLuongConLai ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SoLuotDaBan", model.SoLuotDaBan ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@GiamGia", model.GiamGia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Id", model.MaMonAn);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }

    public async Task<bool> DeleteMonAnAsync(int id)
    {
        var query = "DELETE FROM MonAn WHERE MaMonAn = @Id";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }
}
