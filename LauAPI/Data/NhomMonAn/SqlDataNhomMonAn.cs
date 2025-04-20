using LauAPI.Model;
using Microsoft.Data.SqlClient; 
using System.Threading.Tasks;

public class DishGroupDataAccess
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public DishGroupDataAccess(IConfiguration configuration)
    {
        // Lấy chuỗi kết nối từ appsettings.json ----------------------------------------------------
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("ApplicationDbContext");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured correctly.");
        }
    }
    public async Task<bool> AddDishGroupAsync(NhomMonAn model)
    {
        var query = "INSERT INTO NhomMonAn (TenNhom, HinhAnh) VALUES (@TenNhom, @HinhAnh)";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@TenNhom", model.TenNhom ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }

    public async Task<bool> UpdateDishGroupAsync(NhomMonAn model)
    {
        var updateQuery = string.IsNullOrEmpty(model.HinhAnh)
            ? "UPDATE NhomMonAn SET TenNhom = @TenNhom WHERE MaNhomMonAn = @Id"
            : "UPDATE NhomMonAn SET TenNhom = @TenNhom, HinhAnh = @HinhAnh WHERE MaNhomMonAn = @Id";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(updateQuery, connection))
        {
            command.Parameters.AddWithValue("@TenNhom", model.TenNhom ?? (object)DBNull.Value);
            if (!string.IsNullOrEmpty(model.HinhAnh))
            {
                command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);
            }
            command.Parameters.AddWithValue("@Id", model.MaNhomMonAn);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }


    public async Task<bool> DeleteDishGroupAsync(int id)
    {
        var query = "DELETE FROM NhomMonAn WHERE MaNhomMonAn = @Id";

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
