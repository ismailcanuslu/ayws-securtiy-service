using Ayws.Security.Service.Domain.Entities.Common;

namespace Ayws.Security.Service.Domain.Entities.Tenant;

public class ApiKeyEntity : BaseEntity<Guid>, IAuditEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>SHA-256 hash — asıl değer yalnızca oluşturulurken döner</summary>
    public string KeyHash { get; set; } = null!;

    /// <summary>Prefix: ilk 8 karakter; UI'da göstermek için</summary>
    public string Prefix { get; set; } = null!;

    /// <summary>JSON dizi: ["read:users", "write:tenants"]</summary>
    public string Scopes { get; set; } = "[]";

    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    /// <summary>Rotation sırasında true → 24s geçiş süresi</summary>
    public bool IsRotating { get; set; } = false;
    public Guid? RotationPredecessorId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public TenantEntity Tenant { get; set; } = null!;
}
