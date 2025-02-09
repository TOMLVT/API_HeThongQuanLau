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
        return Ok(nhomMonAn);
    }
}
