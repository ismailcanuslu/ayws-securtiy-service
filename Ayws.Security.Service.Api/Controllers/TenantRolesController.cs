using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Role.Commands.AssignRoles;
using Ayws.Security.Service.Application.Features.Role.Commands.CreateRole;
using Ayws.Security.Service.Application.Features.Role.Commands.DeleteRole;
using Ayws.Security.Service.Application.Features.Role.Commands.UnassignRole;
using Ayws.Security.Service.Application.Features.Role.Commands.UpdateRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/roles")]
public class TenantRolesController(IMediator mediator) : CustomBaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateRoleBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new CreateRoleCommand(tenantId, body.RoleName, body.Description), ct));

    [HttpPut("{roleName}")]
    public async Task<IActionResult> Update(Guid tenantId, string roleName, [FromBody] UpdateRoleBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new UpdateRoleCommand(tenantId, roleName, body.Description), ct));

    [HttpDelete("{roleName}")]
    public async Task<IActionResult> Delete(Guid tenantId, string roleName, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DeleteRoleCommand(tenantId, roleName), ct));

    [HttpPost("/api/tenants/{tenantId:guid}/users/{userId}/roles")]
    public async Task<IActionResult> Assign(Guid tenantId, string userId, [FromBody] AssignRolesBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new AssignRolesCommand(tenantId, userId, body.RoleNames), ct));

    [HttpDelete("/api/tenants/{tenantId:guid}/users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> Unassign(Guid tenantId, string userId, string roleName, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new UnassignRoleCommand(tenantId, userId, roleName), ct));
}

public record CreateRoleBody(string RoleName, string? Description);
public record UpdateRoleBody(string? Description);
public record AssignRolesBody(List<string> RoleNames);
