using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Domain.Entities.Tenant;

public class CertificateEntity : BaseEntity<Guid>, IAuditEntity
{
    public Guid TenantId { get; set; }

    public string Domain { get; set; } = null!;

    public CertificateType Type { get; set; }

    /// <summary>PEM formatında sertifika (şifreli)</summary>
    public string EncryptedPem { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public TenantEntity Tenant { get; set; } = null!;
}

public enum CertificateType
{
    LetsEncrypt,
    SelfSigned,
    Custom
}
