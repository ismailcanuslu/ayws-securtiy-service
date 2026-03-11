using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Commands.DeleteUser;

public record DeleteUserCommand(Guid TenantId, string UserId) : IRequest<ServiceResult>;
