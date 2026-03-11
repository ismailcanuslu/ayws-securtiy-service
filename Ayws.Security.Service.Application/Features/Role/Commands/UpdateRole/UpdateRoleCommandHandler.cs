using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Role.Commands.UpdateRole;

public class UpdateRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<UpdateRoleCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.UpdateRoleAsync(tenant.RealmName, request.RoleName, request.Description, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
