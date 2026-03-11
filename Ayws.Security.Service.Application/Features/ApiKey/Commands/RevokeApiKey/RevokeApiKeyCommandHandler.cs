using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.ApiKey.Commands.RevokeApiKey;

public class RevokeApiKeyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RevokeApiKeyCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var key = await unitOfWork.Repository<ApiKeyEntity, Guid>().GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (key is null || key.TenantId != request.TenantId)
            return ServiceResult.Fail("API Key bulunamadı.", HttpStatusCode.NotFound);

        key.IsRevoked = true;
        unitOfWork.Repository<ApiKeyEntity, Guid>().Update(key);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.SuccessAsNoContent();
    }
}
