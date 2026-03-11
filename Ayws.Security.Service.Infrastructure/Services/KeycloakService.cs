using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;


namespace Ayws.Security.Service.Infrastructure.Services;

/// <summary>
/// Keycloak Admin REST API ile iletişim kurar.
/// HttpClient üzerinden Keycloak Admin REST API'yi çağırır.
/// Keycloak.AuthServices.Sdk token edinimi için kullanılır,
/// Admin API çağrıları basit HttpClient ile yapılır.
/// </summary>
public class KeycloakService(
    IHttpClientFactory httpClientFactory,
    ILogger<KeycloakService> logger) : IKeycloakService
{
    private HttpClient CreateAdminClient() => httpClientFactory.CreateClient("KeycloakAdmin");

    // ─── Realm ──────────────────────────────────────────────────────────────

    public async Task CreateRealmAsync(CreateRealmRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating Keycloak realm: {RealmName}", request.RealmName);
        var client = CreateAdminClient();

        var realmBody = new
        {
            realm = request.RealmName,
            displayName = request.DisplayName,
            enabled = true
        };

        var response = await client.PostAsJsonAsync("/admin/realms", realmBody, ct);
        response.EnsureSuccessStatusCode();

        // Varsayılan roller
        var defaultRoles = new[] { "owner", "admin", "developer", "billing", "readonly" };
        foreach (var role in defaultRoles)
        {
            await client.PostAsJsonAsync(
                $"/admin/realms/{request.RealmName}/roles",
                new { name = role }, ct);
        }
    }

    public async Task DeleteRealmAsync(string realmName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var response = await client.DeleteAsync($"/admin/realms/{realmName}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DisableRealmAsync(string realmName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}", new { enabled = false }, ct);
    }

    public async Task EnableRealmAsync(string realmName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}", new { enabled = true }, ct);
    }

    public async Task UpdateRealmSettingsAsync(UpdateRealmSettingsRequest request, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{request.RealmName}", new
        {
            ssoSessionIdleTimeout = request.SessionIdleTimeoutMinutes * 60,
            accessTokenLifespan = request.AccessTokenLifespanMinutes * 60,
            registrationAllowed = request.RegistrationAllowed
        }, ct);
    }

    // ─── Users ──────────────────────────────────────────────────────────────

    public async Task<string> InviteUserAsync(InviteUserRequest request, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var response = await client.PostAsJsonAsync($"/admin/realms/{request.RealmName}/users", new
        {
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            username = request.Email,
            enabled = true,
            requiredActions = new[] { "VERIFY_EMAIL", "UPDATE_PASSWORD" }
        }, ct);

        response.EnsureSuccessStatusCode();

        // Keycloak Location header'dan userId al
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        var userId = location.Split('/').Last();

        foreach (var roleName in request.Roles)
        {
            await client.PostAsJsonAsync(
                $"/admin/realms/{request.RealmName}/users/{userId}/role-mappings/realm",
                new[] { new { name = roleName } }, ct);
        }

        return userId;
    }

    public async Task<List<KeycloakUserDto>> GetUsersAsync(string realmName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var users = await client.GetFromJsonAsync<List<KeycloakUserResult>>(
            $"/admin/realms/{realmName}/users", ct) ?? [];
        return users.Select(u => new KeycloakUserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Enabled)).ToList();
    }

    public async Task DeleteUserAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.DeleteAsync($"/admin/realms/{realmName}/users/{userId}", ct);
    }

    public async Task DisableUserAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}/users/{userId}", new { enabled = false }, ct);
    }

    public async Task ForcePasswordResetAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}/users/{userId}",
            new { requiredActions = new[] { "UPDATE_PASSWORD" } }, ct);
    }

    public async Task<List<KeycloakSessionDto>> GetUserSessionsAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var sessions = await client.GetFromJsonAsync<List<KeycloakSessionResult>>(
            $"/admin/realms/{realmName}/users/{userId}/sessions", ct) ?? [];
        return sessions.Select(s => new KeycloakSessionDto(
            s.Id, s.IpAddress,
            s.Clients?.Values.FirstOrDefault() ?? "unknown",
            DateTimeOffset.FromUnixTimeMilliseconds(s.Start).UtcDateTime)).ToList();
    }

    public async Task TerminateUserSessionsAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.DeleteAsync($"/admin/realms/{realmName}/users/{userId}/sessions", ct);
    }

    // ─── Roles ──────────────────────────────────────────────────────────────

    public async Task<KeycloakRoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var response = await client.PostAsJsonAsync($"/admin/realms/{request.RealmName}/roles",
            new { name = request.RoleName, description = request.Description }, ct);
        response.EnsureSuccessStatusCode();

        var role = await client.GetFromJsonAsync<KeycloakRoleResult>(
            $"/admin/realms/{request.RealmName}/roles/{request.RoleName}", ct);
        return new KeycloakRoleDto(role?.Id ?? string.Empty, role?.Name ?? request.RoleName, role?.Description);
    }

    public async Task UpdateRoleAsync(string realmName, string roleName, string? description, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}/roles/{roleName}",
            new { name = roleName, description }, ct);
    }

    public async Task DeleteRoleAsync(string realmName, string roleName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.DeleteAsync($"/admin/realms/{realmName}/roles/{roleName}", ct);
    }

    public async Task AssignRolesToUserAsync(string realmName, string userId, List<string> roleNames, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var roles = roleNames.Select(r => new { name = r }).ToList();
        await client.PostAsJsonAsync($"/admin/realms/{realmName}/users/{userId}/role-mappings/realm", roles, ct);
    }

    public async Task UnassignRoleFromUserAsync(string realmName, string userId, string roleName, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/admin/realms/{realmName}/users/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(new[] { new { name = roleName } })
        };
        await client.SendAsync(request, ct);
    }

    public async Task<bool> UserHasPermissionAsync(string realmName, string userId, string resource, string action, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var roles = await client.GetFromJsonAsync<List<KeycloakRoleResult>>(
            $"/admin/realms/{realmName}/users/{userId}/role-mappings/realm", ct) ?? [];
        var permissionRole = $"{resource}:{action}";
        return roles.Any(r => r.Name == permissionRole || r.Name == "admin" || r.Name == "owner");
    }

    // ─── MFA ────────────────────────────────────────────────────────────────

    public async Task<TotpSetupResult> EnableTotpAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"/admin/realms/{realmName}/users/{userId}",
            new { requiredActions = new[] { "CONFIGURE_TOTP" } }, ct);

        return new TotpSetupResult(
            Secret: "KEYCLOAK_MANAGED",
            QrCodeUri: $"otpauth://totp/{realmName}:{userId}?secret=MANAGED&issuer={realmName}",
            RecoveryCodes: GenerateRecoveryCodes(8));
    }

    public async Task DisableTotpAsync(string realmName, string userId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var creds = await client.GetFromJsonAsync<List<KeycloakCredential>>(
            $"/admin/realms/{realmName}/users/{userId}/credentials", ct) ?? [];
        var totp = creds.FirstOrDefault(c => c.Type == "totp");
        if (totp is not null)
            await client.DeleteAsync($"/admin/realms/{realmName}/users/{userId}/credentials/{totp.Id}", ct);
    }

    public async Task SetMfaRequiredAsync(string realmName, bool required, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        // OTP policy — realm seviyesinde
        await client.PutAsJsonAsync($"/admin/realms/{realmName}", new
        {
            otpPolicyType = required ? "totp" : "hotp"
        }, ct);
    }

    // ─── OAuth2 Clients ─────────────────────────────────────────────────────

    public async Task<KeycloakClientCredentials> CreateOAuthClientAsync(CreateOAuthClientRequest request, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var response = await client.PostAsJsonAsync($"/admin/realms/{request.RealmName}/clients", new
        {
            clientId = request.ClientId,
            redirectUris = request.RedirectUris,
            serviceAccountsEnabled = true,
            publicClient = false
        }, ct);
        response.EnsureSuccessStatusCode();

        // Secret al
        var secretResp = await client.GetFromJsonAsync<KeycloakClientSecret>(
            $"/admin/realms/{request.RealmName}/clients/{request.ClientId}/client-secret", ct);
        return new KeycloakClientCredentials(request.ClientId, secretResp?.Value ?? string.Empty);
    }

    public async Task DeleteOAuthClientAsync(string realmName, string clientId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        await client.DeleteAsync($"/admin/realms/{realmName}/clients/{clientId}", ct);
    }

    public async Task<KeycloakClientCredentials> RotateClientSecretAsync(string realmName, string clientId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var secretResp = await client.PostAsJsonAsync<object>(
            $"/admin/realms/{realmName}/clients/{clientId}/client-secret", null, ct);
        var secret = await secretResp.Content.ReadFromJsonAsync<KeycloakClientSecret>(ct);
        return new KeycloakClientCredentials(clientId, secret?.Value ?? string.Empty);
    }

    public async Task<KeycloakClientCredentials> GetClientCredentialsAsync(string realmName, string clientId, CancellationToken ct = default)
    {
        var client = CreateAdminClient();
        var secret = await client.GetFromJsonAsync<KeycloakClientSecret>(
            $"/admin/realms/{realmName}/clients/{clientId}/client-secret", ct);
        return new KeycloakClientCredentials(clientId, secret?.Value ?? string.Empty);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static List<string> GenerateRecoveryCodes(int count)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var bytes = new byte[5];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            codes.Add(Convert.ToHexString(bytes).ToLower());
        }
        return codes;
    }

    // Response records
    private record KeycloakUserResult(string Id, string Email, string FirstName, string LastName, bool Enabled);
    private record KeycloakSessionResult(string Id, string IpAddress, long Start, Dictionary<string, string>? Clients);
    private record KeycloakRoleResult(string Id, string Name, string? Description);
    private record KeycloakCredential(string Id, string Type);
    private record KeycloakClientSecret(string? Value);
}
