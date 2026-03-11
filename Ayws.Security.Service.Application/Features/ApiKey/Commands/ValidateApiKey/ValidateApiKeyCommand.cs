using System.Security.Cryptography;
using System.Text;
using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.ValidateApiKey;

public record ValidateApiKeyCommand(string RawKey) : IRequest<ServiceResult<ValidateApiKeyResponseDto>>;
public record ValidateApiKeyResponseDto(bool IsValid, Guid? TenantId, List<string>? Scopes);

public class ValidateApiKeyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ValidateApiKeyCommand, ServiceResult<ValidateApiKeyResponseDto>>
{
    public async Task<ServiceResult<ValidateApiKeyResponseDto>> Handle(ValidateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RawKey)));

        var key = unitOfWork.Repository<ApiKeyEntity, Guid>()
            .Where(k => k.KeyHash == keyHash);

        var entity = key.FirstOrDefault();

        if (entity is null || entity.IsRevoked)
            return ServiceResult<ValidateApiKeyResponseDto>.SuccessAsOk(new ValidateApiKeyResponseDto(false, null, null));

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value < DateTime.UtcNow)
            return ServiceResult<ValidateApiKeyResponseDto>.SuccessAsOk(new ValidateApiKeyResponseDto(false, null, null));

        var scopes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.Scopes) ?? [];

        return ServiceResult<ValidateApiKeyResponseDto>.SuccessAsOk(
            new ValidateApiKeyResponseDto(true, entity.TenantId, scopes));
    }
}
