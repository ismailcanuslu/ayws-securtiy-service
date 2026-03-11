using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenants;

public record GetTenantsQuery : IRequest<ServiceResult<List<TenantResponseDto>>>;
