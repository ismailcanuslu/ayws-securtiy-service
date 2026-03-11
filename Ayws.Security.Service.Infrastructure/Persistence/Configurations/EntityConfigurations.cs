using Ayws.Security.Service.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ayws.Security.Service.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RealmName).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.RealmName).IsUnique();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(x => x.ApiKeys).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Secrets).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Certificates).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.OAuthClients).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKeyEntity>
{
    public void Configure(EntityTypeBuilder<ApiKeyEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.KeyHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(x => x.KeyHash).IsUnique();
        builder.Property(x => x.Prefix).IsRequired().HasMaxLength(12);
        builder.Property(x => x.Scopes).IsRequired().HasColumnType("text");
    }
}

public class SecretConfiguration : IEntityTypeConfiguration<SecretEntity>
{
    public void Configure(EntityTypeBuilder<SecretEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        builder.Property(x => x.EncryptedValue).IsRequired().HasColumnType("text");
    }
}

public class CertificateConfiguration : IEntityTypeConfiguration<CertificateEntity>
{
    public void Configure(EntityTypeBuilder<CertificateEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Domain).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.EncryptedPem).IsRequired().HasColumnType("text");
    }
}

public class OAuthClientConfiguration : IEntityTypeConfiguration<OAuthClientEntity>
{
    public void Configure(EntityTypeBuilder<OAuthClientEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(150);
        builder.HasIndex(x => new { x.TenantId, x.ClientId }).IsUnique();
        builder.Property(x => x.EncryptedSecret).IsRequired().HasColumnType("text");
    }
}
