using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Role.Commands.DeleteRole;

public class DeleteRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<DeleteRoleCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.DeleteRoleAsync(tenant.RealmName, request.RoleName, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
