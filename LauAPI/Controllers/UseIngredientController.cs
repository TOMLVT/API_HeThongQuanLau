
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UseIngredientController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public UseIngredientController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet("use-ingredients")]
    public async Task<IActionResult> GetSuDungNguyenLieu()
    {
        var suDungList = await _sqlDataAccess.GetSuDungNguyenLieuAsync();
        return Ok(suDungList);
    }

}
