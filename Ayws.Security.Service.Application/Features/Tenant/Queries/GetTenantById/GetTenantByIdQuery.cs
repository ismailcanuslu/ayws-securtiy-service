using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid TenantId) : IRequest<ServiceResult<TenantResponseDto>>;
