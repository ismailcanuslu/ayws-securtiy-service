namespace Ayws.Security.Service.Application.Features.Certificate.Dto;

public record CertificateResponseDto(Guid Id, string Domain, string Type, DateTime ExpiresAt, DateTime CreatedAt);
public record CertificateExpiryDto(Guid Id, string Domain, DateTime ExpiresAt, int DaysUntilExpiry);
