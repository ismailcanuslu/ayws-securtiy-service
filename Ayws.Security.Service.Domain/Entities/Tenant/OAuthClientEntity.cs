using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Domain.Entities.Tenant;

public class OAuthClientEntity : BaseEntity<Guid>, IAuditEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Keycloak client_id</summary>
    public string ClientId { get; set; } = null!;

    /// <summary>AES-256-GCM ile şifrelenmiş client_secret</summary>
    public string EncryptedSecret { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public TenantEntity Tenant { get; set; } = null!;
}
