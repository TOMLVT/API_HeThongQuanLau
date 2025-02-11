
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public IngredientController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet("ingredients")]
    public async Task<IActionResult> GetNguyenLieu()
    {
        var nguyenLieuList = await _sqlDataAccess.GetNguyenLieuAsync();
        return Ok(nguyenLieuList);
    }
}
