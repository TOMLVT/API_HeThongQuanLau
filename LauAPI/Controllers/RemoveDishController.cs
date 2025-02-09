using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using LauAPI.Model;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class RemoveDishController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public RemoveDishController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpDelete("remove-dish-table")]
    public async Task<IActionResult> RemoveDishFromTable([FromBody] RemoveDish request)
    {
        try
        {
            bool isRemoved = await _sqlDataAccess.RemoveDishFromOrderAsync(request.MaBan, request.MaMonAn, request.SoLuongXoa);

            if (!isRemoved)
            {
                return NotFound(new { message = "Món ăn không tồn tại hoặc không thể xóa." });
            }

            return Ok(new { message = $"Đã xóa {request.SoLuongXoa} món khỏi bàn {request.MaBan}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa món ăn.", error = ex.Message });
        }
    }

}
