using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DishController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;
    private readonly SqlMonAnAccess _sqlMonAnAccess;

    public DishController(SqlDataAccess sqlDataAccess, SqlMonAnAccess sqlMonAnAccess)
    {
        _sqlDataAccess = sqlDataAccess;
        _sqlMonAnAccess = sqlMonAnAccess;
    }

    [HttpPost("add-to-cart")]
    public async Task<IActionResult> AddToCart([FromBody] OrderRequest request)
    {
        var dish = await _sqlDataAccess.GetDishByIdAsync(request.MaMonAn);
        if (dish == null || dish.SoLuongConLai < request.SoLuong)
        {
            return BadRequest(new { message = "Số lượng không đủ hoặc món ăn không tồn tại." });
        }
        return Ok(new { message = "Thêm vào giỏ hàng thành công" });
    }

    [HttpPost("update-stock-on-order")]
    public async Task<IActionResult> UpdateStockOnOrder([FromBody] List<OrderRequest> items)
    {
        var success = await _sqlMonAnAccess.UpdateStockOnOrderAsync(items);
        if (success)
        {
            return Ok(new { message = "Cập nhật số lượng món ăn thành công" });
        }
        return BadRequest(new { message = "Không thể cập nhật kho. Kiểm tra số lượng còn lại." });
    }

    [HttpPost("rollback-stock")]
    public async Task<IActionResult> RollbackStock([FromBody] List<OrderRequest> items)
    {
        var success = await _sqlMonAnAccess.RollbackStockAsync(items);
        if (success)
        {
            return Ok(new { message = "Rollback số lượng món ăn thành công" });
        }
        return BadRequest(new { message = "Không thể rollback kho." });
    }

    [HttpGet("{maMonAn}")]
    public async Task<IActionResult> GetDish(int maMonAn)
    {
        var dish = await _sqlDataAccess.GetDishByIdAsync(maMonAn);
        if (dish == null)
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }

        if (!string.IsNullOrEmpty(dish.HinhAnh))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/images/";
            var fileName = Path.GetFileName(dish.HinhAnh);
            dish.HinhAnh = $"{baseUrl}{Uri.EscapeDataString(fileName)}";
        }
        return Ok(dish);
    }
    [HttpGet]
    public async Task<IActionResult> GetAllDishes()
    {
        var dishes = await _sqlDataAccess.GetAllDishesAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}/images/";
        foreach (var item in dishes)
        {
            if (!string.IsNullOrEmpty(item.HinhAnh))
            {
                var fileName = Path.GetFileName(item.HinhAnh);
                item.HinhAnh = $"{baseUrl}{Uri.EscapeDataString(fileName)}";
            }
        }
        return Ok(dishes);
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AddDish([FromForm] MonAn monAn, [FromForm] IFormFile image)
    {
        string imagePath = null;

        if (image != null && image.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            imagePath = fileName;
        }

        monAn.HinhAnh = imagePath;

        var result = await _sqlMonAnAccess.AddMonAnAsync(monAn);

        if (result)
        {
            return Ok(new { message = "Thêm món ăn thành công" });
        }
        else
        {
            return BadRequest(new { message = "Thêm món ăn thất bại" });
        }
    }

    [HttpPut("{id}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UpdateDish(int id, [FromForm] MonAn monAn, [FromForm] IFormFile? image)
    {
        string? imagePath = null;

        if (image != null && image.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            imagePath = fileName;
        }

        monAn.MaMonAn = id;
        monAn.HinhAnh = imagePath ?? monAn.HinhAnh;

        var result = await _sqlMonAnAccess.UpdateMonAnAsync(monAn);

        if (result)
        {
            return Ok(new { message = "Cập nhật món ăn thành công ! " });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDish(int id)
    {
        var result = await _sqlMonAnAccess.DeleteMonAnAsync(id);

        if (result)
        {
            return Ok(new { message = "Xóa món ăn thành công" });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }
    }
}