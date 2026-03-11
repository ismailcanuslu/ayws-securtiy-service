using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Features.ApiKey.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.CreateApiKey;

public class CreateApiKeyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateApiKeyCommand, ServiceResult<CreateApiKeyResponseDto>>
{
    public async Task<ServiceResult<CreateApiKeyResponseDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<CreateApiKeyResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        // Güvenli rastgele key üret
        var rawKey = $"ayws_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
        var prefix = rawKey[..12];
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        var entity = new ApiKeyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            KeyHash = keyHash,
            Prefix = prefix,
            Scopes = JsonSerializer.Serialize(request.Scopes),
            ExpiresAt = request.ExpiresAt
        };

        await unitOfWork.Repository<ApiKeyEntity, Guid>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CreateApiKeyResponseDto>.SuccessAsCreated(
            new CreateApiKeyResponseDto(entity.Id, entity.Name, entity.Prefix, rawKey));
    }
}
