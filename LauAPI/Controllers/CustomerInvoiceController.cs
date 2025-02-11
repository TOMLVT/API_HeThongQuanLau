
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class CustomerInvoiceController : ControllerBase
{
    private readonly SqlDataAccess _sqlDataAccess;

    public CustomerInvoiceController(SqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    [HttpGet("customer-invoice")]
    public async Task<IActionResult> GetHoaDonKhachHang()
    {
        var hoaDonList = await _sqlDataAccess.GetHoaDonKhachHangAsync();
        return Ok(hoaDonList);
    }

}
