using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Role.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Role.Commands.CreateRole;

public record CreateRoleCommand(Guid TenantId, string RoleName, string? Description) : IRequest<ServiceResult<RoleResponseDto>>;
