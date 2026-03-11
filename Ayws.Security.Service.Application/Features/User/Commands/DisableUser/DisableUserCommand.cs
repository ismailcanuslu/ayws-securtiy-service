using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Commands.DisableUser;

public record DisableUserCommand(Guid TenantId, string UserId) : IRequest<ServiceResult>;
