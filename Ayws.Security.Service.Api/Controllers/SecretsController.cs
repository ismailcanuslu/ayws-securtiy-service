using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Secret.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/secrets")]
public class SecretsController(IMediator mediator) : CustomBaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateSecretBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new CreateSecretCommand(tenantId, body.Key, body.Value), ct));

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(Guid tenantId, string key, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetSecretQuery(tenantId, key), ct));

    [HttpPost("{key}/rotate")]
    public async Task<IActionResult> Rotate(Guid tenantId, string key, [FromBody] RotateSecretBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new RotateSecretCommand(tenantId, key, body.NewValue), ct));
}

public record CreateSecretBody(string Key, string Value);
public record RotateSecretBody(string NewValue);
