using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.ApiKey.Commands.CreateApiKey;
using Ayws.Security.Service.Application.Features.ApiKey.Commands.RevokeApiKey;
using Ayws.Security.Service.Application.Features.ApiKey.Commands.RotateApiKey;
using Ayws.Security.Service.Application.Features.ApiKey.Commands.ValidateApiKey;
using Ayws.Security.Service.Application.Features.ApiKey.Queries.GetApiKeys;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/api-keys")]
public class ApiKeysController(IMediator mediator) : CustomBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetApiKeysQuery(tenantId), ct));

    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateApiKeyCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command with { TenantId = tenantId }, ct);
        return CreateActionResult(result, nameof(GetAll), new { tenantId });
    }

    [HttpDelete("{keyId:guid}")]
    public async Task<IActionResult> Revoke(Guid tenantId, Guid keyId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new RevokeApiKeyCommand(tenantId, keyId), ct));

    [HttpPost("{keyId:guid}/rotate")]
    public async Task<IActionResult> Rotate(Guid tenantId, Guid keyId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new RotateApiKeyCommand(tenantId, keyId), ct));
}

[Route("api/api-keys")]
public class ApiKeyValidationController(IMediator mediator) : CustomBaseController
{
    /// <summary>Internal — sadece api-gateway çağırır</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateApiKeyBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new ValidateApiKeyCommand(body.RawKey), ct));
}

public record ValidateApiKeyBody(string RawKey);
