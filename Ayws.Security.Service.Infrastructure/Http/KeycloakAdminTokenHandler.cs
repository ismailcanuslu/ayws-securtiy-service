using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Ayws.Security.Service.Infrastructure.Http;

/// <summary>
/// Her Admin API isteğinden önce client_credentials grant ile Bearer token alır.
/// Token 60 saniye cache'lenir (basit in-memory yaklaşım).
/// </summary>
public class KeycloakAdminTokenHandler(IConfiguration configuration) : DelegatingHandler
{
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check locking
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var tokenUrl = configuration["KeycloakSettings:TokenUrl"]
                ?? "http://localhost:8080/realms/master/protocol/openid-connect/token";
            var clientId = configuration["KeycloakSettings:AdminClientId"] ?? "ayws-admin";
            var clientSecret = configuration["KeycloakSettings:AdminClientSecret"] ?? "";

            using var http = new HttpClient();
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var response = await http.PostAsync(tokenUrl, new FormUrlEncodedContent(form), ct);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
            _cachedToken = tokenResponse!.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // 30s buffer

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
