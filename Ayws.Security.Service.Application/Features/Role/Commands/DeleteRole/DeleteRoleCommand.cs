using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Role.Commands.DeleteRole;

public record DeleteRoleCommand(Guid TenantId, string RoleName) : IRequest<ServiceResult>;
