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


    [HttpGet]
    public async Task<IActionResult> GetDishs(int maMonAn)
    {
        var tables = await _sqlDataAccess.GetDishesAsync(maMonAn);

        var baseUrl = $"{Request.Scheme}://{Request.Host}/images/";
        foreach (var item in tables)
        {
            if (!string.IsNullOrEmpty(item.HinhAnh))
            {
                var fileName = Path.GetFileName(item.HinhAnh);
                item.HinhAnh = $"{baseUrl}{Uri.EscapeDataString(fileName)}";
            }
        }
        return Ok(tables);
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