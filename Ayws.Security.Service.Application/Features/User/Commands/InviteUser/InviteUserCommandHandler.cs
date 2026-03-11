using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.User.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.User.Commands.InviteUser;

public class InviteUserCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<InviteUserCommand, ServiceResult<UserResponseDto>>
{
    public async Task<ServiceResult<UserResponseDto>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<UserResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var userId = await keycloakService.InviteUserAsync(new InviteUserRequest(
            tenant.RealmName,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Roles), cancellationToken);

        return ServiceResult<UserResponseDto>.SuccessAsCreated(
            new UserResponseDto(userId, request.Email, request.FirstName, request.LastName, true));
    }
}
