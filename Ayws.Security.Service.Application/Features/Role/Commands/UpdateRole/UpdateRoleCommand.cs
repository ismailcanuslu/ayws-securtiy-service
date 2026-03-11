using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Role.Commands.UpdateRole;

public record UpdateRoleCommand(Guid TenantId, string RoleName, string? Description) : IRequest<ServiceResult>;
