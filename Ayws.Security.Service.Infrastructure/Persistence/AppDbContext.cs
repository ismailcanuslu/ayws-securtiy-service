using Ayws.Security.Service.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Ayws.Security.Service.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Tenant
    public DbSet<TenantEntity> Tenants { get; set; }
    #endregion

    #region ApiKey
    public DbSet<ApiKeyEntity> ApiKeys { get; set; }
    #endregion

    #region Secret
    public DbSet<SecretEntity> Secrets { get; set; }
    #endregion

    #region Certificate
    public DbSet<CertificateEntity> Certificates { get; set; }
    #endregion

    #region OAuthClient
    public DbSet<OAuthClientEntity> OAuthClients { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
