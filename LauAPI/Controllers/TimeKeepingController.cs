using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TimeKeepingController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public TimeKeepingController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkToDay()
    {
        var tables = await _sqlDataAccess.GetworkAsync();
        return Ok(tables);
    }

    // API POST chấm công nhân viên
    [HttpPost("time-keeping")]
    public async Task<IActionResult> ChamCongNhanVien([FromBody] ChamCongNhanVien chamCong)
    {
        if (chamCong.MaNV <= 0)
        {
            return BadRequest(new { message = "Mã nhân viên không hợp lệ!" });
        }

        try
        {
            await _sqlDataAccess.ChamCongNhanVienAsync(chamCong);

            return Ok(new { message = "Chấm công nhân viên thành công!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }
}
