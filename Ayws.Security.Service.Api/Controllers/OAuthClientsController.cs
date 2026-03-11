using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.OAuthClient.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/oauth-clients")]
public class OAuthClientsController(IMediator mediator) : CustomBaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateOAuthClientCommand command, CancellationToken ct)
        => CreateActionResult(await mediator.Send(command with { TenantId = tenantId }, ct));

    [HttpDelete("{clientId:guid}")]
    public async Task<IActionResult> Delete(Guid tenantId, Guid clientId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DeleteOAuthClientCommand(tenantId, clientId), ct));

    [HttpPost("{clientId:guid}/rotate-secret")]
    public async Task<IActionResult> RotateSecret(Guid tenantId, Guid clientId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new RotateOAuthClientSecretCommand(tenantId, clientId), ct));

    [HttpGet("{clientId:guid}/credentials")]
    public async Task<IActionResult> GetCredentials(Guid tenantId, Guid clientId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetOAuthClientCredentialsQuery(tenantId, clientId), ct));
}
