using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Mfa.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/users/{userId}")]
public class MfaController(IMediator mediator) : CustomBaseController
{
    [HttpPost("mfa/enable")]
    public async Task<IActionResult> Enable(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new EnableMfaCommand(tenantId, userId), ct));

    [HttpDelete("mfa")]
    public async Task<IActionResult> Disable(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DisableMfaCommand(tenantId, userId), ct));

    [HttpPost("mfa/recovery-codes")]
    public async Task<IActionResult> GenerateRecoveryCodes(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GenerateMfaRecoveryCodesCommand(tenantId, userId), ct));

    [HttpPut("/api/tenants/{tenantId:guid}/mfa/require")]
    public async Task<IActionResult> SetRequired(Guid tenantId, [FromBody] SetMfaRequiredBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new SetMfaRequiredCommand(tenantId, body.Required), ct));
}

public record SetMfaRequiredBody(bool Required);
