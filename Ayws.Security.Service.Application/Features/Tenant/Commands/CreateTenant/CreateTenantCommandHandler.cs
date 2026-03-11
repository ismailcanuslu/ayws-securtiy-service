using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using AutoMapper;
using MediatR;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(
    IUnitOfWork unitOfWork,
    IKeycloakService keycloakService,
    IMapper mapper) : IRequestHandler<CreateTenantCommand, ServiceResult<TenantResponseDto>>
{
    public async Task<ServiceResult<TenantResponseDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var exists = await unitOfWork.Repository<TenantEntity, Guid>()
            .AnyAsync(t => t.RealmName == request.RealmName, cancellationToken);

        if (exists)
            return ServiceResult<TenantResponseDto>.Fail("Bu RealmName zaten kullanımda.");

        // Keycloak'ta realm oluştur
        await keycloakService.CreateRealmAsync(new CreateRealmRequest(request.RealmName, request.DisplayName), cancellationToken);

        // Local DB'ye kaydet
        var tenant = new TenantEntity
        {
            Id = Guid.NewGuid(),
            RealmName = request.RealmName,
            DisplayName = request.DisplayName,
            Status = TenantStatus.Active
        };

        await unitOfWork.Repository<TenantEntity, Guid>().AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TenantResponseDto>.SuccessAsCreated(mapper.Map<TenantResponseDto>(tenant));
    }
}
