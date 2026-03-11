using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Permission.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/internal")]
public class InternalController(IMediator mediator) : CustomBaseController
{
    /// <summary>Sadece iç ağdan erişilebilir — api-gateway çağırır</summary>
    [HttpPost("check-permission")]
    public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionCommand command, CancellationToken ct)
        => CreateActionResult(await mediator.Send(command, ct));
}
