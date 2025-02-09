using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace LauAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // Adding this will make sure your controller works with routing, binding, etc.
    public class OrderController : ControllerBase
    {
        private readonly SqlDataAccess _sqlDataAccess;

        public OrderController(SqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        [HttpPost("add-dish-to-table")]
        public async Task<IActionResult> AddDishToTable([FromBody] OrderRequest request)
        {
            try
            {
                // Kiểm tra xem bàn có tồn tại không
                var table = await _sqlDataAccess.GetTablesAsync(request.MaBan);
                if (table == null)
                {
                    return NotFound(new { message = "Bàn không tồn tại!" });
                }

                // Kiểm tra món ăn có tồn tại không
                var dish = await _sqlDataAccess.GetDishesAsync(request.MaMonAn);

                if (dish == null)
                {
                    return NotFound(new { message = "Món ăn không tồn tại!" });
                }

                // Thêm món ăn vào hóa đơn và xử lý trong transaction
                var result = await _sqlDataAccess.AddDishToOrderAsync(request);
                if (result.Contains("Lỗi"))
                {
                    return StatusCode(500, new { message = "Lỗi khi thêm món!", error = result });
                }

                var updateResult = await _sqlDataAccess.UpdateTableStatusAsync(request.MaBan, "Đang sử dụng");

                if (!updateResult)
                {
                    return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái bàn!" });
                }

                return Ok(new { message = "Thêm món ăn thành công vào bàn!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm món!", error = ex.Message });
            }
        }


    }
}
