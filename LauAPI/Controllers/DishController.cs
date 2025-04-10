using LauAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DishController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public DishController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
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
}