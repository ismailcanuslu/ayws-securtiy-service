using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.UpdateTenantSettings;

public record UpdateTenantSettingsCommand(
    Guid TenantId,
    int SessionIdleTimeoutMinutes,
    int AccessTokenLifespanMinutes,
    bool RegistrationAllowed) : IRequest<ServiceResult>;
