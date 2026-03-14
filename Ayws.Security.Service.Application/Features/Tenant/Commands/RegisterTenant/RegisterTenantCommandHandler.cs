using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.RegisterTenant;

public class RegisterTenantCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<RegisterTenantCommand, ServiceResult<RegisterTenantResponseDto>>
{
    public async Task<ServiceResult<RegisterTenantResponseDto>> Handle(
        RegisterTenantCommand request,
        CancellationToken cancellationToken)
    {
        // 1) Keycloak realm oluştur
        await keycloakService.CreateRealmAsync(
            new CreateRealmRequest(request.RealmName, request.DisplayName),
            cancellationToken);

        string keycloakUserId;
        try
        {
            // 2) Owner kullanıcısını oluştur ve owner rolünü ata
            keycloakUserId = await keycloakService.RegisterUserAsync(
                new RegisterUserRequest(
                    request.RealmName,
                    request.OwnerEmail,
                    request.OwnerFirstName,
                    request.OwnerLastName,
                    request.OwnerPassword),
                cancellationToken);
        }
        catch
        {
            // Kullanıcı oluşturulamazsa realm'ı geri al
            await keycloakService.DeleteRealmAsync(request.RealmName, cancellationToken);
            throw;
        }

        // 3) Local DB'ye tenant kaydet
        var tenant = new TenantEntity
        {
            Id = Guid.NewGuid(),
            RealmName = request.RealmName,
            DisplayName = request.DisplayName,
            OwnerEmail = request.OwnerEmail.ToLowerInvariant(),
            Status = TenantStatus.Active
        };

        await unitOfWork.Repository<TenantEntity, Guid>().AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<RegisterTenantResponseDto>.SuccessAsCreated(
            new RegisterTenantResponseDto(tenant.Id, tenant.RealmName, keycloakUserId));
    }
}
