using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TablesController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public TablesController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet]
    public async Task<IActionResult> GetTables()
    {
        var tables = await _sqlDataAccess.GetTablesAsync();
        return Ok(tables);
    }
}
