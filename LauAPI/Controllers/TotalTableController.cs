using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using LauAPI.Model;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class TotalTableController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public TotalTableController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet("get-total-table/{maBan}")]
    public async Task<IActionResult> GetHoaDonByMaBan(int maBan)
    {
        try
        {
            var hoaDonList = await _sqlDataAccess.GetHoaDonBanHangAsync(maBan);

            if (hoaDonList == null || hoaDonList.Count == 0)
            {
                return NotFound(new { message = "Không có hóa đơn cho bàn này!" });
            }

            // Tổng tiền bàn đã được tính trong GetHoaDonBanHangAsync
            decimal tongTienBan = hoaDonList.FirstOrDefault()?.TongTienBan ?? 0;

            return Ok(new
            {
                MaBan = maBan,
                TongTienBan = tongTienBan, // Lấy tổng tiền của bàn từ các hóa đơn
                HoaDonList = hoaDonList
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Lỗi khi lấy hóa đơn!", error = ex.Message 
            });
        }
    }
}
