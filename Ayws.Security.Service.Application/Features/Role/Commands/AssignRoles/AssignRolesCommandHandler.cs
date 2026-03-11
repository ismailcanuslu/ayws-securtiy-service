using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Role.Commands.AssignRoles;

public class AssignRolesCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<AssignRolesCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.AssignRolesToUserAsync(tenant.RealmName, request.UserId, request.RoleNames, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
