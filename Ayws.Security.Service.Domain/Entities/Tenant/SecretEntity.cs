using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Domain.Entities.Tenant;

public class SecretEntity : BaseEntity<Guid>, IAuditEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Anahtar adı — tenant içinde unique</summary>
    public string Key { get; set; } = null!;

    /// <summary>AES-256-GCM ile şifrelenmiş değer (Base64)</summary>
    public string EncryptedValue { get; set; } = null!;

    public DateTime? RotatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public TenantEntity Tenant { get; set; } = null!;
}
