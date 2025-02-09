using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LauAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SumBillController : ControllerBase
    {
        private readonly SqlDataAccess _sqlDataAccess;

        public SumBillController(SqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        [HttpPost("ThanhToan")] // Thanh toán bàn từ 21 -> 26 
        public async Task<IActionResult> ThanhToanAsync([FromBody] ThanhToan request)
        {
            if (request.MaBan <= 0)
            {
                return BadRequest("Vui lòng chọn bàn trước khi thanh toán.");
            }

            try
            {
                decimal totalAmount = await _sqlDataAccess.ThanhToanAsync(request);
                return Ok(new { message = "Thanh toán bàn ăn thành công ! ", tongTien = totalAmount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi thanh toán: {ex.Message}");
            }
        }

    }
}
