using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using AutoMapper;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GetTenantByIdQuery, ServiceResult<TenantResponseDto>>
{
    public async Task<ServiceResult<TenantResponseDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<TenantResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        return ServiceResult<TenantResponseDto>.SuccessAsOk(mapper.Map<TenantResponseDto>(tenant));
    }
}
