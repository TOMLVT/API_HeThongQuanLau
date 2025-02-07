using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using LauAPI.Model;
using System.Security.Cryptography;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public AuthController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    // 🔹 API lấy danh sách nhân viên
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _sqlDataAccess.GetAllUsersAsync();
        return Ok(users);
    }

    // 🔹 API đăng nhập
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
      
        var user = await _sqlDataAccess.LoginAsync(request.Email, request.MatKhau);
        if (user == null)
        {
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });
        }

        return Ok(new
        {
            MaNV = user.MaNV,
            HoTen = user.HoTen,
            Email = user.Email,
            MaPQ = user.MaPQ,
        });
    }

    // 🔹 API đăng ký
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
      
        var newUser = new NhanVien
        {
            HoTen = request.HoTen,
            Email = request.Email,
            SDT = request.SDT,
            MatKhau = request.MatKhau,  // Lưu mật khẩu đã được hash
            MaPQ = request.MaPQ
        };

        bool success = await _sqlDataAccess.RegisterAsync(newUser);
        if (!success)
        {
            return BadRequest(new { message = "Email đã tồn tại!" });
        }

        return Ok(new { message = "Đăng ký thành công!" });
    }
}

// 🔹 Model cho LoginRequest
public class LoginRequest
{
    public string Email { get; set; }
    public string MatKhau { get; set; }
}

// 🔹 Model cho RegisterRequest
public class RegisterRequest
{
    public string HoTen { get; set; }
    public string Email { get; set; }
    public string SDT { get; set; }
    public string MatKhau { get; set; }
    public int MaPQ { get; set; }
}