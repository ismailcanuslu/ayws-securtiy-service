using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.User.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.User.Queries.GetUsers;

public class GetUsersQueryHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<GetUsersQuery, ServiceResult<List<UserResponseDto>>>
{
    public async Task<ServiceResult<List<UserResponseDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<List<UserResponseDto>>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var users = await keycloakService.GetUsersAsync(tenant.RealmName, cancellationToken);
        var result = users.Select(u => new UserResponseDto(u.Id, u.Email, u.FirstName, u.LastName, u.Enabled)).ToList();

        return ServiceResult<List<UserResponseDto>>.SuccessAsOk(result);
    }
}
