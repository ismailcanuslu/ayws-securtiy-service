namespace Ayws.Security.Service.Application.Features.Mfa.Dto;

public record MfaSetupResponseDto(
    string Secret,
    string QrCodeUri,
    List<string> RecoveryCodes
);
