using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.SuspendTenant;

public record SuspendTenantCommand(Guid TenantId) : IRequest<ServiceResult>;
