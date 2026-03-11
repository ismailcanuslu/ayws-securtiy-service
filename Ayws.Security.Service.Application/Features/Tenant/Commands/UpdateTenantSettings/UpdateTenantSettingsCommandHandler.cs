using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.UpdateTenantSettings;

public class UpdateTenantSettingsCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<UpdateTenantSettingsCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(UpdateTenantSettingsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.UpdateRealmSettingsAsync(new UpdateRealmSettingsRequest(
            tenant.RealmName,
            request.SessionIdleTimeoutMinutes,
            request.AccessTokenLifespanMinutes,
            request.RegistrationAllowed), cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}
