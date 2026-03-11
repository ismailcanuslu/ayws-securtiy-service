using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Role.Commands.AssignRoles;

public record AssignRolesCommand(Guid TenantId, string UserId, List<string> RoleNames) : IRequest<ServiceResult>;
