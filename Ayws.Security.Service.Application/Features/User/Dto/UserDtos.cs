namespace Ayws.Security.Service.Application.Features.User.Dto;

public record UserResponseDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool Enabled
);

public record SessionResponseDto(
    string Id,
    string IpAddress,
    string Browser,
    DateTime Started
);
