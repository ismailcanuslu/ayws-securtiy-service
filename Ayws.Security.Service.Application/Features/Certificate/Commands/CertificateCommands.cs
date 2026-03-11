using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Encryption;
using Ayws.Security.Service.Application.Features.Certificate.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using System.Net;

namespace Ayws.Security.Service.Application.Features.Certificate.Commands;

// ─── Create Certificate ───────────────────────────────────────────────────────
public record CreateCertificateCommand(
    Guid TenantId,
    string Domain,
    CertificateType Type,
    string PemContent,
    DateTime ExpiresAt) : IRequest<ServiceResult<CertificateResponseDto>>;

public class CreateCertificateCommandHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService) : IRequestHandler<CreateCertificateCommand, ServiceResult<CertificateResponseDto>>
{
    public async Task<ServiceResult<CertificateResponseDto>> Handle(CreateCertificateCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<TenantEntity, Guid>().GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return ServiceResult<CertificateResponseDto>.Fail("Tenant bulunamadı.", HttpStatusCode.NotFound);

        var entity = new CertificateEntity
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Domain = request.Domain,
            Type = request.Type,
            EncryptedPem = encryptionService.Encrypt(request.PemContent),
            ExpiresAt = request.ExpiresAt
        };

        await unitOfWork.Repository<CertificateEntity, Guid>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CertificateResponseDto>.SuccessAsCreated(
            new CertificateResponseDto(entity.Id, entity.Domain, entity.Type.ToString(), entity.ExpiresAt, entity.CreatedAt));
    }
}

// ─── Get Certificate Expiry ──────────────────────────────────────────────────
public record GetCertificateExpiryQuery(Guid TenantId, Guid CertificateId) : IRequest<ServiceResult<CertificateExpiryDto>>;

public class GetCertificateExpiryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCertificateExpiryQuery, ServiceResult<CertificateExpiryDto>>
{
    public async Task<ServiceResult<CertificateExpiryDto>> Handle(GetCertificateExpiryQuery request, CancellationToken cancellationToken)
    {
        var cert = await unitOfWork.Repository<CertificateEntity, Guid>().GetByIdAsync(request.CertificateId, cancellationToken);
        if (cert is null || cert.TenantId != request.TenantId)
            return ServiceResult<CertificateExpiryDto>.Fail("Sertifika bulunamadı.", HttpStatusCode.NotFound);

        var daysUntilExpiry = (int)(cert.ExpiresAt - DateTime.UtcNow).TotalDays;
        return ServiceResult<CertificateExpiryDto>.SuccessAsOk(
            new CertificateExpiryDto(cert.Id, cert.Domain, cert.ExpiresAt, daysUntilExpiry));
    }
}
