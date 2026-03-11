using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Encryption;
using Ayws.Security.Service.Application.Features.Secret.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Secret.Commands;

// ─── Create Secret ───────────────────────────────────────────────────────────
public record CreateSecretCommand(Guid TenantId, string Key, string Value) : IRequest<ServiceResult<Guid>>;

public class CreateSecretCommandHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService) : IRequestHandler<CreateSecretCommand, ServiceResult<Guid>>
{
    public async Task<ServiceResult<Guid>> Handle(CreateSecretCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<Guid>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var alreadyExists = await unitOfWork.Repository<SecretEntity, Guid>()
            .AnyAsync(s => s.TenantId == request.TenantId && s.Key == request.Key, cancellationToken);
        if (alreadyExists)
            return ServiceResult<Guid>.Fail("Bu anahtar adı zaten kullanımda.");

        var entity = new SecretEntity
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Key = request.Key,
            EncryptedValue = encryptionService.Encrypt(request.Value)
        };

        await unitOfWork.Repository<SecretEntity, Guid>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<Guid>.SuccessAsCreated(entity.Id);
    }
}

// ─── Get Secret ──────────────────────────────────────────────────────────────
public record GetSecretQuery(Guid TenantId, string Key) : IRequest<ServiceResult<SecretResponseDto>>;

public class GetSecretQueryHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService) : IRequestHandler<GetSecretQuery, ServiceResult<SecretResponseDto>>
{
    public async Task<ServiceResult<SecretResponseDto>> Handle(GetSecretQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<SecretResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var secret = unitOfWork.Repository<SecretEntity, Guid>()
            .Where(s => s.TenantId == request.TenantId && s.Key == request.Key)
            .FirstOrDefault();

        if (secret is null)
            return ServiceResult<SecretResponseDto>.Fail("Secret bulunamadı.", HttpStatusCode.NotFound);

        var decrypted = encryptionService.Decrypt(secret.EncryptedValue);
        return ServiceResult<SecretResponseDto>.SuccessAsOk(
            new SecretResponseDto(secret.Id, secret.Key, decrypted, secret.RotatedAt, secret.CreatedAt));
    }
}

// ─── Rotate Secret ───────────────────────────────────────────────────────────
public record RotateSecretCommand(Guid TenantId, string Key, string NewValue) : IRequest<ServiceResult>;

public class RotateSecretCommandHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService) : IRequestHandler<RotateSecretCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(RotateSecretCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var secret = unitOfWork.Repository<SecretEntity, Guid>()
            .Where(s => s.TenantId == request.TenantId && s.Key == request.Key)
            .FirstOrDefault();

        if (secret is null)
            return ServiceResult.Fail("Secret bulunamadı.", HttpStatusCode.NotFound);

        secret.EncryptedValue = encryptionService.Encrypt(request.NewValue);
        secret.RotatedAt = DateTime.UtcNow;
        unitOfWork.Repository<SecretEntity, Guid>().Update(secret);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}
