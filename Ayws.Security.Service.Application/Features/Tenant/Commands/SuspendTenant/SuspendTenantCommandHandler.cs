using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.SuspendTenant;

public class SuspendTenantCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<SuspendTenantCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        if (tenant.Status == TenantStatus.Suspended)
            return ServiceResult.Fail("Tenant zaten askıya alınmış.");

        await keycloakService.DisableRealmAsync(tenant.RealmName, cancellationToken);
        tenant.Status = TenantStatus.Suspended;
        unitOfWork.Repository<TenantEntity, Guid>().Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}
