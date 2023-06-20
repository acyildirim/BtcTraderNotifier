using BtcTrader.Core;
using Microsoft.AspNetCore.Mvc;

namespace BtcTrader.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseApiController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if(result == null) return NotFound();
        if (result.IsSuccess && result.Value != null)
            return Ok(result);
        if (result.IsSuccess && result.Value != null && result.Message != null)
            return Ok(result);
        if (result.IsSuccess && result.Value == null)
            return NotFound();
        return BadRequest(result);
    }
        
}
