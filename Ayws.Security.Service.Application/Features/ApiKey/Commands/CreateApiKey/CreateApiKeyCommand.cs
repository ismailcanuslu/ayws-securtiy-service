using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.ApiKey.Dto;
using MediatR;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.CreateApiKey;

public record CreateApiKeyCommand(
    Guid TenantId,
    string Name,
    List<string> Scopes,
    DateTime? ExpiresAt) : IRequest<ServiceResult<CreateApiKeyResponseDto>>;
