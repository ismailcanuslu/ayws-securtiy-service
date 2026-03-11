using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.Role.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Role.Commands.CreateRole;

public class CreateRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<CreateRoleCommand, ServiceResult<RoleResponseDto>>
{
    public async Task<ServiceResult<RoleResponseDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<RoleResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var role = await keycloakService.CreateRoleAsync(new CreateRoleRequest(tenant.RealmName, request.RoleName, request.Description), cancellationToken);
        return ServiceResult<RoleResponseDto>.SuccessAsCreated(new RoleResponseDto(role.Id, role.Name, role.Description));
    }
}
