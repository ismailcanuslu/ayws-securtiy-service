using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Features.ApiKey.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.RotateApiKey;

public record RotateApiKeyCommand(Guid TenantId, Guid ApiKeyId) : IRequest<ServiceResult<CreateApiKeyResponseDto>>;

public class RotateApiKeyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RotateApiKeyCommand, ServiceResult<CreateApiKeyResponseDto>>
{
    public async Task<ServiceResult<CreateApiKeyResponseDto>> Handle(RotateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var oldKey = await unitOfWork.Repository<ApiKeyEntity, Guid>().GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (oldKey is null || oldKey.TenantId != request.TenantId)
            return ServiceResult<CreateApiKeyResponseDto>.Fail("API Key bulunamadı.", HttpStatusCode.NotFound);

        if (oldKey.IsRevoked)
            return ServiceResult<CreateApiKeyResponseDto>.Fail("İptal edilmiş bir API Key rotate edilemez.");

        // Yeni key üret
        var rawKey = $"ayws_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
        var prefix = rawKey[..12];
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        // Eski key'i rotating moduna al (24s geçiş süresi)
        oldKey.IsRotating = true;
        unitOfWork.Repository<ApiKeyEntity, Guid>().Update(oldKey);

        // Yeni key ekle
        var newKey = new ApiKeyEntity
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = oldKey.Name,
            KeyHash = keyHash,
            Prefix = prefix,
            Scopes = oldKey.Scopes,
            ExpiresAt = oldKey.ExpiresAt,
            RotationPredecessorId = oldKey.Id
        };

        await unitOfWork.Repository<ApiKeyEntity, Guid>().AddAsync(newKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CreateApiKeyResponseDto>.SuccessAsCreated(
            new CreateApiKeyResponseDto(newKey.Id, newKey.Name, newKey.Prefix, rawKey));
    }
}
