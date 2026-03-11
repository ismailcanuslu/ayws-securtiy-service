using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Role.Commands.UnassignRole;

public record UnassignRoleCommand(Guid TenantId, string UserId, string RoleName) : IRequest<ServiceResult>;
