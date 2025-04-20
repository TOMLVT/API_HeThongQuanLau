using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DishGroupController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;
    private readonly DishGroupDataAccess _sqlNhomMonAn;

    public DishGroupController(SqlDataAccess sqlDataAccess , DishGroupDataAccess sqlNhomMonAn)
    {
        _sqlDataAccess = sqlDataAccess;
        _sqlNhomMonAn = sqlNhomMonAn;
    }

    [HttpGet]
  
    public async Task<IActionResult> GetDishGroup()
    {
        var nhomMonAn = await _sqlDataAccess.GetDishGroupAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}/images/";
        foreach (var item in nhomMonAn)
        {
            if (!string.IsNullOrEmpty(item.HinhAnh))
            {
                var fileName = Path.GetFileName(item.HinhAnh);
                item.HinhAnh = $"{baseUrl}{Uri.EscapeDataString(fileName)}";
            }
        }

        return Ok(nhomMonAn);
    }


    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AddDishGroup([FromForm] string tenNhom, [FromForm] IFormFile image)
    {
        string imagePath = null;

        if (image != null && image.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images"); // đảm bảo đúng folder public
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            imagePath = fileName; // Lưu tên file thôi!
        }

        var model = new NhomMonAn
        {
            TenNhom = tenNhom,
            HinhAnh = imagePath
        };

        var result = await _sqlNhomMonAn.AddDishGroupAsync(model);

        if (result)
        {
            return Ok(new { message = "Thêm nhóm món ăn thành công" });
        }
        else
        {
            return BadRequest(new { message = "Thêm thất bại" });
        }
    }


    [HttpPut("{id}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UpdateDishGroup(int id, [FromForm] string tenNhom, [FromForm] IFormFile? image)
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

        var model = new NhomMonAn
        {
            MaNhomMonAn = id,
            TenNhom = tenNhom,
            HinhAnh = imagePath
        };

        var result = await _sqlNhomMonAn.UpdateDishGroupAsync(model);

        if (result)
        {
            return Ok(new { message = "Cập nhật nhóm món ăn thành công" });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy nhóm món ăn" });
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDishGroup(int id)
    {
        var result = await _sqlNhomMonAn.DeleteDishGroupAsync(id);

        if (result)
        {
            return Ok(new { message = "Xóa nhóm món ăn thành công ! " });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy nhóm món ăn" });
        }
    }

}
