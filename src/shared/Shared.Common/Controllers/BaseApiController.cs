using Microsoft.AspNetCore.Mvc;

namespace Shared.Common.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null) return NotFound();

        if (result.Error != Error.None)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result == null) return NotFound();

        if (result.Error != Error.None)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
