namespace Ayws.Security.Service.Application.Features.ApiKey.Dto;

public record ApiKeyResponseDto(
    Guid Id,
    string Name,
    string Prefix,
    List<string> Scopes,
    DateTime? ExpiresAt,
    bool IsRevoked,
    bool IsRotating,
    DateTime CreatedAt
);

public record CreateApiKeyResponseDto(
    Guid Id,
    string Name,
    string Prefix,
    /// <summary>Yalnızca oluşturulurken döner — sonradan görünemez</summary>
    string RawKey
);
