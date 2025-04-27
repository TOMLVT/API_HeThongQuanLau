using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AnhSliderController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;
    private readonly AnhSliderDataAccess _sqlAnhSlider;

    public AnhSliderController(SqlDataAccess sqlDataAccess, AnhSliderDataAccess sqlAnhSlider)
    {
        _sqlDataAccess = sqlDataAccess;
        _sqlAnhSlider = sqlAnhSlider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnhSlider()
    {
        var anhSlider = await _sqlDataAccess.GetAnhSliderAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}/images/";
        foreach (var item in anhSlider)
        {
            if (!string.IsNullOrEmpty(item.HinhAnh))
            {
                var fileName = Path.GetFileName(item.HinhAnh);
                item.HinhAnh = $"{baseUrl}{Uri.EscapeDataString(fileName)}";
            }
        }

        return Ok(anhSlider);
    }

   


    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AddAnhSlider([FromForm] IFormFile image)
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

        var model = new AnhSlider
        {
            HinhAnh = imagePath
        };

        var result = await _sqlAnhSlider.AddAnhSliderAsync(model);

        if (result)
        {
            return Ok(new { message = "Thêm ảnh slider thành công" });
        }
        else
        {
            return BadRequest(new { message = "Thêm thất bại" });
        }
    }

    [HttpPut("{id}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UpdateAnhSlider(int id, [FromForm] IFormFile? image)
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

        var model = new AnhSlider
        {
            MaSlider = id,
            HinhAnh = imagePath
        };

        var result = await _sqlAnhSlider.UpdateAnhSliderAsync(model);

        if (result)
        {
            return Ok(new { message = "Cập nhật ảnh slider thành công" });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy ảnh slider" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnhSlider(int id)
    {
        var result = await _sqlAnhSlider.DeleteAnhSliderAsync(id);

        if (result)
        {
            return Ok(new { message = "Xóa ảnh slider thành công!" });
        }
        else
        {
            return NotFound(new { message = "Không tìm thấy ảnh slider" });
        }
    }
}
