using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.Permission.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Permission.Commands;

public record CheckPermissionCommand(
    string TenantId,
    string UserId,
    string Resource,
    string Action) : IRequest<ServiceResult<CheckPermissionResponseDto>>;

public class CheckPermissionCommandHandler(IKeycloakService keycloakService)
    : IRequestHandler<CheckPermissionCommand, ServiceResult<CheckPermissionResponseDto>>
{
    public async Task<ServiceResult<CheckPermissionResponseDto>> Handle(CheckPermissionCommand request, CancellationToken cancellationToken)
    {
        var allowed = await keycloakService.UserHasPermissionAsync(
            request.TenantId,
            request.UserId,
            request.Resource,
            request.Action,
            cancellationToken);

        return ServiceResult<CheckPermissionResponseDto>.SuccessAsOk(new CheckPermissionResponseDto(allowed));
    }
}
