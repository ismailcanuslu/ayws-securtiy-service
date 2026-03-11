using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.User.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Queries.GetUsers;

public record GetUsersQuery(Guid TenantId) : IRequest<ServiceResult<List<UserResponseDto>>>;
