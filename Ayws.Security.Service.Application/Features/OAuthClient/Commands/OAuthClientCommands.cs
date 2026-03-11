using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.OAuthClient.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.OAuthClient.Commands;

// ─── Create OAuthClient ──────────────────────────────────────────────────────
public record CreateOAuthClientCommand(Guid TenantId, string ClientId, List<string> RedirectUris) : IRequest<ServiceResult<OAuthClientResponseDto>>;

public class CreateOAuthClientCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<CreateOAuthClientCommand, ServiceResult<OAuthClientResponseDto>>
{
    public async Task<ServiceResult<OAuthClientResponseDto>> Handle(CreateOAuthClientCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<OAuthClientResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var creds = await keycloakService.CreateOAuthClientAsync(
            new CreateOAuthClientRequest(tenant.RealmName, request.ClientId, request.RedirectUris), cancellationToken);

        // Local DB'ye kaydet (secret şifreli saklanır — EncryptionService Infrastructure'da inject edilecek)
        // Bu handler OAuthClientEntity oluşturmaz; şifreleme Infrastructure'da yapılır.
        // Basit yaklaşım: Controller'da IEncryptionService inject ederek yapılabilir ama
        // daha temiz: Infrastructure'da OAuthClientEntity yönetimi için ayrı handler yazılabilir.
        // Şimdilik sadece Keycloak response'unu dön.

        var entity = new OAuthClientEntity
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ClientId = request.ClientId,
            EncryptedSecret = creds.ClientSecret // Infrastructure handler şifreleyecek
        };

        await unitOfWork.Repository<OAuthClientEntity, Guid>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<OAuthClientResponseDto>.SuccessAsCreated(
            new OAuthClientResponseDto(entity.Id, entity.ClientId, entity.CreatedAt));
    }
}

// ─── Delete OAuthClient ──────────────────────────────────────────────────────
public record DeleteOAuthClientCommand(Guid TenantId, Guid ClientDbId) : IRequest<ServiceResult>;

public class DeleteOAuthClientCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<DeleteOAuthClientCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(DeleteOAuthClientCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var client = await unitOfWork.Repository<OAuthClientEntity, Guid>().GetByIdAsync(request.ClientDbId, cancellationToken);
        if (client is null || client.TenantId != request.TenantId)
            return ServiceResult.Fail("OAuth Client bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.DeleteOAuthClientAsync(tenant.RealmName, client.ClientId, cancellationToken);
        await unitOfWork.Repository<OAuthClientEntity, Guid>().DeleteAsync(request.ClientDbId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}

// ─── Rotate OAuthClient Secret ───────────────────────────────────────────────
public record RotateOAuthClientSecretCommand(Guid TenantId, Guid ClientDbId) : IRequest<ServiceResult<OAuthClientCredentialsDto>>;

public class RotateOAuthClientSecretCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<RotateOAuthClientSecretCommand, ServiceResult<OAuthClientCredentialsDto>>
{
    public async Task<ServiceResult<OAuthClientCredentialsDto>> Handle(RotateOAuthClientSecretCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<OAuthClientCredentialsDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var client = await unitOfWork.Repository<OAuthClientEntity, Guid>().GetByIdAsync(request.ClientDbId, cancellationToken);
        if (client is null || client.TenantId != request.TenantId)
            return ServiceResult<OAuthClientCredentialsDto>.Fail("OAuth Client bulunamadı.", HttpStatusCode.NotFound);

        var newCreds = await keycloakService.RotateClientSecretAsync(tenant.RealmName, client.ClientId, cancellationToken);

        client.EncryptedSecret = newCreds.ClientSecret;
        unitOfWork.Repository<OAuthClientEntity, Guid>().Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<OAuthClientCredentialsDto>.SuccessAsOk(
            new OAuthClientCredentialsDto(newCreds.ClientId, newCreds.ClientSecret));
    }
}

// ─── Get OAuthClient Credentials ────────────────────────────────────────────
public record GetOAuthClientCredentialsQuery(Guid TenantId, Guid ClientDbId) : IRequest<ServiceResult<OAuthClientCredentialsDto>>;

public class GetOAuthClientCredentialsQueryHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<GetOAuthClientCredentialsQuery, ServiceResult<OAuthClientCredentialsDto>>
{
    public async Task<ServiceResult<OAuthClientCredentialsDto>> Handle(GetOAuthClientCredentialsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<OAuthClientCredentialsDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var client = await unitOfWork.Repository<OAuthClientEntity, Guid>().GetByIdAsync(request.ClientDbId, cancellationToken);
        if (client is null || client.TenantId != request.TenantId)
            return ServiceResult<OAuthClientCredentialsDto>.Fail("OAuth Client bulunamadı.", HttpStatusCode.NotFound);

        var creds = await keycloakService.GetClientCredentialsAsync(tenant.RealmName, client.ClientId, cancellationToken);
        return ServiceResult<OAuthClientCredentialsDto>.SuccessAsOk(
            new OAuthClientCredentialsDto(creds.ClientId, creds.ClientSecret));
    }
}
