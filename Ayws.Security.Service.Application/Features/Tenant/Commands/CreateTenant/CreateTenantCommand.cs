using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.CreateTenant;

public record CreateTenantCommand(string RealmName, string DisplayName) : IRequest<ServiceResult<TenantResponseDto>>;
