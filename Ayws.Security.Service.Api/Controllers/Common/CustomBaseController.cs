using Ayws.Security.Service.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class CustomBaseController : ControllerBase
{
    protected IActionResult CreateActionResult(ServiceResult result)
        => result.IsSuccess
            ? StatusCode((int)result.StatusCode)
            : StatusCode((int)result.StatusCode, new { errors = result.ErrorMessages });

    protected IActionResult CreateActionResult<T>(ServiceResult<T> result)
        => result.IsSuccess
            ? StatusCode((int)result.StatusCode, result.Data)
            : StatusCode((int)result.StatusCode, new { errors = result.ErrorMessages });

    protected IActionResult CreateActionResult<T>(ServiceResult<T> result, string? actionName, object? routeValues)
    {
        if (!result.IsSuccess)
            return StatusCode((int)result.StatusCode, new { errors = result.ErrorMessages });

        return CreatedAtAction(actionName, routeValues, result.Data);
    }
}
