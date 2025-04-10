using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DishGroupController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public DishGroupController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
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


}
