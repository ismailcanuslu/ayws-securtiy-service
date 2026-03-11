using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Tenant.Commands.CreateTenant;
using Ayws.Security.Service.Application.Features.Tenant.Commands.DeleteTenant;
using Ayws.Security.Service.Application.Features.Tenant.Commands.ReactivateTenant;
using Ayws.Security.Service.Application.Features.Tenant.Commands.SuspendTenant;
using Ayws.Security.Service.Application.Features.Tenant.Commands.UpdateTenantSettings;
using Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenantById;
using Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants")]
public class TenantsController(IMediator mediator) : CustomBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetTenantsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetTenantByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreateActionResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DeleteTenantCommand(id), ct));

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new SuspendTenantCommand(id), ct));

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new ReactivateTenantCommand(id), ct));

    [HttpPut("{id:guid}/settings")]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateTenantSettingsRequest request, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new UpdateTenantSettingsCommand(
            id, request.SessionIdleTimeoutMinutes, request.AccessTokenLifespanMinutes, request.RegistrationAllowed), ct));
}

public record UpdateTenantSettingsRequest(int SessionIdleTimeoutMinutes, int AccessTokenLifespanMinutes, bool RegistrationAllowed);
