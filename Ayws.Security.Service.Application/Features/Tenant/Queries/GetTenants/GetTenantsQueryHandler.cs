using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using AutoMapper;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenants;

public class GetTenantsQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GetTenantsQuery, ServiceResult<List<TenantResponseDto>>>
{
    public async Task<ServiceResult<List<TenantResponseDto>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await unitOfWork.Repository<TenantEntity, Guid>().GetAllListAsync(cancellationToken);
        return ServiceResult<List<TenantResponseDto>>.SuccessAsOk(mapper.Map<List<TenantResponseDto>>(tenants));
    }
}
