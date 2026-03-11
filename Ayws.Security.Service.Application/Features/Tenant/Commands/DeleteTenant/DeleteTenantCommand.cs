using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.DeleteTenant;

public record DeleteTenantCommand(Guid TenantId) : IRequest<ServiceResult>;
