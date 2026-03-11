namespace Ayws.Security.Service.Application.Features.Tenant.Dto;

public record TenantResponseDto(
    Guid Id,
    string RealmName,
    string DisplayName,
    string Status,
    DateTime CreatedAt
);
