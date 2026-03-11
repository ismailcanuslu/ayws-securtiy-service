namespace Ayws.Security.Service.Application.Features.Secret.Dto;

public record SecretResponseDto(Guid Id, string Key, string DecryptedValue, DateTime? RotatedAt, DateTime CreatedAt);
