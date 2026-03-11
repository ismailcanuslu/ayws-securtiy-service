using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Domain.Entities.Tenant;

public class TenantEntity : BaseEntity<Guid>, IAuditEntity
{
    /// <summary>Keycloak realm adı (unique)</summary>
    public string RealmName { get; set; } = null!;

    /// <summary>İnsan-okunabilir görünen ad</summary>
    public string DisplayName { get; set; } = null!;

    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<ApiKeyEntity> ApiKeys { get; set; } = new();
    public List<SecretEntity> Secrets { get; set; } = new();
    public List<CertificateEntity> Certificates { get; set; } = new();
    public List<OAuthClientEntity> OAuthClients { get; set; } = new();
}

public enum TenantStatus
{
    Active,
    Suspended
}
