using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.User.Commands.DeleteUser;
using Ayws.Security.Service.Application.Features.User.Commands.DisableUser;
using Ayws.Security.Service.Application.Features.User.Commands.ForcePasswordReset;
using Ayws.Security.Service.Application.Features.User.Commands.InviteUser;
using Ayws.Security.Service.Application.Features.User.Commands.TerminateSessions;
using Ayws.Security.Service.Application.Features.User.Queries.GetUserSessions;
using Ayws.Security.Service.Application.Features.User.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/users")]
public class TenantUsersController(IMediator mediator) : CustomBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetUsersQuery(tenantId), ct));

    [HttpPost("invite")]
    public async Task<IActionResult> Invite(Guid tenantId, [FromBody] InviteUserCommand command, CancellationToken ct)
        => CreateActionResult(await mediator.Send(command with { TenantId = tenantId }, ct));

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DeleteUserCommand(tenantId, userId), ct));

    [HttpPost("{userId}/disable")]
    public async Task<IActionResult> Disable(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new DisableUserCommand(tenantId, userId), ct));

    [HttpPost("{userId}/force-password-reset")]
    public async Task<IActionResult> ForcePasswordReset(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new ForcePasswordResetCommand(tenantId, userId), ct));

    [HttpGet("{userId}/sessions")]
    public async Task<IActionResult> GetSessions(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetUserSessionsQuery(tenantId, userId), ct));

    [HttpDelete("{userId}/sessions")]
    public async Task<IActionResult> TerminateSessions(Guid tenantId, string userId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new TerminateSessionsCommand(tenantId, userId), ct));
}
