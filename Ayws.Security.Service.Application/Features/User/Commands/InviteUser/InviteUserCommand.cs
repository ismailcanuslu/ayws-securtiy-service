using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.User.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Commands.InviteUser;

public record InviteUserCommand(
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    List<string> Roles) : IRequest<ServiceResult<UserResponseDto>>;
