using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.RegisterTenant;

public record RegisterTenantCommand(
    string RealmName,
    string DisplayName,
    string OwnerEmail,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerPassword) : IRequest<ServiceResult<RegisterTenantResponseDto>>;
