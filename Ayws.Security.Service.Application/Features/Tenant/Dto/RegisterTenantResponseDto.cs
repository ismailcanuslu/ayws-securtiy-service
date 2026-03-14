namespace Ayws.Security.Service.Application.Features.Tenant.Dto;

public record RegisterTenantResponseDto(Guid TenantId, string RealmName, string KeycloakUserId);
