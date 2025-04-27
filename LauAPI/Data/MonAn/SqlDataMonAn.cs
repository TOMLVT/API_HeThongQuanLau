using Microsoft.Extensions.Configuration;
using LauAPI.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
            command.Parameters.AddWithValue("@SoLuongConLai", model.SoLuongConLai);  
            command.Parameters.AddWithValue("@SoLuotDaBan", model.SoLuotDaBan);
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
            command.Parameters.AddWithValue("@SoLuongConLai", model.SoLuongConLai);
            command.Parameters.AddWithValue("@SoLuotDaBan", model.SoLuotDaBan);
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

    public async Task<bool> UpdateQuantityAndSoldAsync(int maMonAn, int soLuong)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Kiểm tra món ăn tồn tại và số lượng đủ
                    var checkQuery = "SELECT SoLuongConLai FROM MonAn WHERE MaMonAn = @MaMonAn";
                    using (var checkCommand = new SqlCommand(checkQuery, connection, transaction))
                    {
                        checkCommand.Parameters.AddWithValue("@MaMonAn", maMonAn);
                        var result = await checkCommand.ExecuteScalarAsync();
                        if (result == null || Convert.ToInt32(result) < soLuong)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                    }

                    // Cập nhật số lượng
                    var updateQuery = @"
                    UPDATE MonAn
                    SET 
                        SoLuongConLai = SoLuongConLai - @SoLuong,
                        SoLuotDaBan = COALESCE(SoLuotDaBan, 0) + @SoLuong
                    WHERE 
                        MaMonAn = @MaMonAn";

                    using (var updateCommand = new SqlCommand(updateQuery, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@MaMonAn", maMonAn);
                        updateCommand.Parameters.AddWithValue("@SoLuong", soLuong);
                        var affectedRows = await updateCommand.ExecuteNonQueryAsync();

                        if (affectedRows == 0)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
    public async Task<bool> RollbackStockAsync(List<OrderRequest> items)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in items)
                    {
                        var query = @"
                        UPDATE MonAn
                        SET 
                            SoLuongConLai = SoLuongConLai + @SoLuong,
                            SoLuotDaBan = COALESCE(SoLuotDaBan, 0) - @SoLuong
                        WHERE 
                            MaMonAn = @MaMonAn";

                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaMonAn", item.MaMonAn);
                            command.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            var affectedRows = await command.ExecuteNonQueryAsync();

                            if (affectedRows == 0)
                            {
                                await transaction.RollbackAsync();
                                return false;
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
    public async Task<bool> UpdateStockOnOrderAsync(List<OrderRequest> items)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in items)
                    {
                        // Kiểm tra món ăn tồn tại và số lượng đủ
                        var checkQuery = "SELECT SoLuongConLai FROM MonAn WHERE MaMonAn = @MaMonAn";
                        using (var checkCommand = new SqlCommand(checkQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@MaMonAn", item.MaMonAn);
                            var result = await checkCommand.ExecuteScalarAsync();
                            if (result == null || Convert.ToInt32(result) < item.SoLuong)
                            {
                                await transaction.RollbackAsync();
                                return false;
                            }
                        }

                        // Cập nhật số lượng
                        var updateQuery = @"
                        UPDATE MonAn
                        SET 
                            SoLuongConLai = SoLuongConLai - @SoLuong,
                            SoLuotDaBan = COALESCE(SoLuotDaBan, 0) + @SoLuong
                        WHERE 
                            MaMonAn = @MaMonAn";

                        using (var updateCommand = new SqlCommand(updateQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@MaMonAn", item.MaMonAn);
                            updateCommand.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            var affectedRows = await updateCommand.ExecuteNonQueryAsync();

                            if (affectedRows == 0)
                            {
                                await transaction.RollbackAsync();
                                return false;
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}