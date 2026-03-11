using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Role.Commands.UnassignRole;

public class UnassignRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<UnassignRoleCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(UnassignRoleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.UnassignRoleFromUserAsync(tenant.RealmName, request.UserId, request.RoleName, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
