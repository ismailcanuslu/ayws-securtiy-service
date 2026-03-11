using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.User.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.User.Queries.GetUserSessions;

public class GetUserSessionsQueryHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService) : IRequestHandler<GetUserSessionsQuery, ServiceResult<List<SessionResponseDto>>>
{
    public async Task<ServiceResult<List<SessionResponseDto>>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<List<SessionResponseDto>>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var sessions = await keycloakService.GetUserSessionsAsync(tenant.RealmName, request.UserId, cancellationToken);
        var result = sessions.Select(s => new SessionResponseDto(s.Id, s.IpAddress, s.Browser, s.Started)).ToList();

        return ServiceResult<List<SessionResponseDto>>.SuccessAsOk(result);
    }
}
