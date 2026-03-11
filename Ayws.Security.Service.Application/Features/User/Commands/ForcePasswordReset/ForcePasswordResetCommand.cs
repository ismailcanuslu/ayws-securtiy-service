using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Commands.ForcePasswordReset;

public record ForcePasswordResetCommand(Guid TenantId, string UserId) : IRequest<ServiceResult>;
