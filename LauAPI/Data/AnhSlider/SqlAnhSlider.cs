using LauAPI.Model;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public class AnhSliderDataAccess
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public AnhSliderDataAccess(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("ApplicationDbContext");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured correctly.");
        }
    }

    public async Task<bool> AddAnhSliderAsync(AnhSlider model)
    {
        var query = "INSERT INTO AnhSlider (HinhAnh) VALUES (@HinhAnh)";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }

    public async Task<bool> UpdateAnhSliderAsync(AnhSlider model)
    {
        var updateQuery = string.IsNullOrEmpty(model.HinhAnh)
            ? "UPDATE AnhSlider SET HinhAnh = @HinhAnh WHERE MaSlider = @Id"
            : "UPDATE AnhSlider SET HinhAnh = @HinhAnh WHERE MaSlider = @Id";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(updateQuery, connection))
        {
            command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Id", model.MaSlider);

            await connection.OpenAsync();
            var affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }
    }

    public async Task<bool> DeleteAnhSliderAsync(int id)
    {
        var query = "DELETE FROM AnhSlider WHERE MaSlider = @Id";

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
