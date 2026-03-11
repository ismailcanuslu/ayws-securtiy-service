namespace Ayws.Security.Service.Application.Features.OAuthClient.Dto;

public record OAuthClientResponseDto(Guid Id, string ClientId, DateTime CreatedAt);
public record OAuthClientCredentialsDto(string ClientId, string ClientSecret);
