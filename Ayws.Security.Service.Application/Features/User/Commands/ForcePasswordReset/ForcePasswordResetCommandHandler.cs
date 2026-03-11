using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.User.Commands.ForcePasswordReset;

public class ForcePasswordResetCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<ForcePasswordResetCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(ForcePasswordResetCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        await keycloakService.ForcePasswordResetAsync(tenant.RealmName, request.UserId, cancellationToken);
        return ServiceResult.SuccessAsNoContent();
    }
}
