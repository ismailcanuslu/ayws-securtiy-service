using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.DeleteTenant;

public class DeleteTenantCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<DeleteTenantCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await keycloakService.DeleteRealmAsync(tenant.RealmName, cancellationToken);
        await unitOfWork.Repository<TenantEntity, Guid>().DeleteAsync(request.TenantId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}
