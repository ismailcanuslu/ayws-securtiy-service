using Ayws.Security.Service.Application.Common;
using MediatR;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.RevokeApiKey;

public record RevokeApiKeyCommand(Guid TenantId, Guid ApiKeyId) : IRequest<ServiceResult>;
