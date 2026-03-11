using System.Text.Json;
using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Features.ApiKey.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.ApiKey.Queries.GetApiKeys;

public record GetApiKeysQuery(Guid TenantId) : IRequest<ServiceResult<List<ApiKeyResponseDto>>>;

public class GetApiKeysQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetApiKeysQuery, ServiceResult<List<ApiKeyResponseDto>>>
{
    public async Task<ServiceResult<List<ApiKeyResponseDto>>> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<List<ApiKeyResponseDto>>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var keys = unitOfWork.Repository<ApiKeyEntity, Guid>()
            .Where(k => k.TenantId == request.TenantId)
            .Select(k => new ApiKeyResponseDto(
                k.Id, k.Name, k.Prefix,
                JsonSerializer.Deserialize<List<string>>(k.Scopes) ?? new(),
                k.ExpiresAt, k.IsRevoked, k.IsRotating, k.CreatedAt))
            .ToList();

        return ServiceResult<List<ApiKeyResponseDto>>.SuccessAsOk(keys);
    }
}
