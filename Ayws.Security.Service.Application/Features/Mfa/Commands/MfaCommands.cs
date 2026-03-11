using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.Mfa.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Mfa.Commands;

// ─── Enable MFA ─────────────────────────────────────────────────────────────
public record EnableMfaCommand(Guid TenantId, string UserId) : IRequest<ServiceResult<MfaSetupResponseDto>>;

public class EnableMfaCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<EnableMfaCommand, ServiceResult<MfaSetupResponseDto>>
{
    public async Task<ServiceResult<MfaSetupResponseDto>> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<MfaSetupResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var result = await keycloakService.EnableTotpAsync(tenant.RealmName, request.UserId, cancellationToken);
        return ServiceResult<MfaSetupResponseDto>.SuccessAsOk(
            new MfaSetupResponseDto(result.Secret, result.QrCodeUri, result.RecoveryCodes));
    }
}

// ─── Disable MFA ────────────────────────────────────────────────────────────
public record DisableMfaCommand(Guid TenantId, string UserId) : IRequest<ServiceResult>;

public class DisableMfaCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<DisableMfaCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.DisableTotpAsync(tenant.RealmName, request.UserId, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}

// ─── Generate Recovery Codes ─────────────────────────────────────────────────
public record GenerateMfaRecoveryCodesCommand(Guid TenantId, string UserId) : IRequest<ServiceResult<List<string>>>;

public class GenerateMfaRecoveryCodesCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<GenerateMfaRecoveryCodesCommand, ServiceResult<List<string>>>
{
    public async Task<ServiceResult<List<string>>> Handle(GenerateMfaRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<List<string>>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var result = await keycloakService.EnableTotpAsync(tenant.RealmName, request.UserId, cancellationToken);
        return ServiceResult<List<string>>.SuccessAsOk(result.RecoveryCodes);
    }
}

// ─── Set MFA Required ───────────────────────────────────────────────────────
public record SetMfaRequiredCommand(Guid TenantId, bool Required) : IRequest<ServiceResult>;

public class SetMfaRequiredCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<SetMfaRequiredCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(SetMfaRequiredCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.SetMfaRequiredAsync(tenant.RealmName, request.Required, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
