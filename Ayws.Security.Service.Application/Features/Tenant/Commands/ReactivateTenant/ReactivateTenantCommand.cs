using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.ReactivateTenant;

public record ReactivateTenantCommand(Guid TenantId) : IRequest<ServiceResult>;
