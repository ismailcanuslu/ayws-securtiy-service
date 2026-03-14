namespace Ayws.Security.Service.Application.Contracts.Services.Keycloak;

// ─── Tenant / Realm DTOs ────────────────────────────────────────────────────
public record CreateRealmRequest(string RealmName, string DisplayName);
public record UpdateRealmSettingsRequest(string RealmName, int SessionIdleTimeoutMinutes, int AccessTokenLifespanMinutes, bool RegistrationAllowed);

// ─── User DTOs ──────────────────────────────────────────────────────────────
public record InviteUserRequest(string RealmName, string Email, string FirstName, string LastName, List<string> Roles);
public record RegisterUserRequest(string RealmName, string Email, string FirstName, string LastName, string Password);
public record KeycloakUserDto(string Id, string Email, string FirstName, string LastName, bool Enabled);
public record KeycloakSessionDto(string Id, string IpAddress, string Browser, DateTime Started);

// ─── Role DTOs ──────────────────────────────────────────────────────────────
public record CreateRoleRequest(string RealmName, string RoleName, string? Description);
public record KeycloakRoleDto(string Id, string Name, string? Description);

// ─── OAuth2 Client DTOs ─────────────────────────────────────────────────────
public record CreateOAuthClientRequest(string RealmName, string ClientId, List<string> RedirectUris);
public record KeycloakClientCredentials(string ClientId, string ClientSecret);

// ─── MFA DTOs ───────────────────────────────────────────────────────────────
public record TotpSetupResult(string Secret, string QrCodeUri, List<string> RecoveryCodes);

public interface IKeycloakService
{
    // Realm (Tenant)
    Task CreateRealmAsync(CreateRealmRequest request, CancellationToken ct = default);
    Task DeleteRealmAsync(string realmName, CancellationToken ct = default);
    Task DisableRealmAsync(string realmName, CancellationToken ct = default);
    Task EnableRealmAsync(string realmName, CancellationToken ct = default);
    Task UpdateRealmSettingsAsync(UpdateRealmSettingsRequest request, CancellationToken ct = default);

    // Users
    Task<string> InviteUserAsync(InviteUserRequest request, CancellationToken ct = default);
    Task<string> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<List<KeycloakUserDto>> GetUsersAsync(string realmName, CancellationToken ct = default);
    Task DeleteUserAsync(string realmName, string userId, CancellationToken ct = default);
    Task DisableUserAsync(string realmName, string userId, CancellationToken ct = default);
    Task ForcePasswordResetAsync(string realmName, string userId, CancellationToken ct = default);
    Task<List<KeycloakSessionDto>> GetUserSessionsAsync(string realmName, string userId, CancellationToken ct = default);
    Task TerminateUserSessionsAsync(string realmName, string userId, CancellationToken ct = default);

    // Roles
    Task<KeycloakRoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task UpdateRoleAsync(string realmName, string roleName, string? description, CancellationToken ct = default);
    Task DeleteRoleAsync(string realmName, string roleName, CancellationToken ct = default);
    Task AssignRolesToUserAsync(string realmName, string userId, List<string> roleNames, CancellationToken ct = default);
    Task UnassignRoleFromUserAsync(string realmName, string userId, string roleName, CancellationToken ct = default);
    Task<bool> UserHasPermissionAsync(string realmName, string userId, string resource, string action, CancellationToken ct = default);

    // MFA
    Task<TotpSetupResult> EnableTotpAsync(string realmName, string userId, CancellationToken ct = default);
    Task DisableTotpAsync(string realmName, string userId, CancellationToken ct = default);
    Task SetMfaRequiredAsync(string realmName, bool required, CancellationToken ct = default);

    // OAuth2 Clients
    Task<KeycloakClientCredentials> CreateOAuthClientAsync(CreateOAuthClientRequest request, CancellationToken ct = default);
    Task DeleteOAuthClientAsync(string realmName, string clientId, CancellationToken ct = default);
    Task<KeycloakClientCredentials> RotateClientSecretAsync(string realmName, string clientId, CancellationToken ct = default);
    Task<KeycloakClientCredentials> GetClientCredentialsAsync(string realmName, string clientId, CancellationToken ct = default);
}
