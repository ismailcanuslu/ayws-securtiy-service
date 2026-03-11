using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.User.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Queries.GetUserSessions;

public record GetUserSessionsQuery(Guid TenantId, string UserId) : IRequest<ServiceResult<List<SessionResponseDto>>>;
