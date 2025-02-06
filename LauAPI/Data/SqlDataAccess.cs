using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

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
}

public class Table
{
    public int MaBan { get; set; }
    public string TenBan { get; set; }
    public string TrangThai { get; set; }
    public string TenKV { get; set; }
}
