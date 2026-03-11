using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.User.Commands.TerminateSessions;

public record TerminateSessionsCommand(Guid TenantId, string UserId) : IRequest<ServiceResult>;
