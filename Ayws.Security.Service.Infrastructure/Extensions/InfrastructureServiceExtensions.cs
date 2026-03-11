using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Contracts.Services.Encryption;
using Ayws.Security.Service.Application.Contracts.Services.Keycloak;
using Ayws.Security.Service.Infrastructure.Http;
using Ayws.Security.Service.Infrastructure.Persistence;
using Ayws.Security.Service.Infrastructure.Persistence.Interceptors;
using Ayws.Security.Service.Infrastructure.Persistence.Repositories;
using Ayws.Security.Service.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ayws.Security.Service.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        services.AddSingleton<AuditDbContextInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(InfrastructureAssembly).Assembly.FullName));
            options.AddInterceptors(sp.GetRequiredService<AuditDbContextInterceptor>());
        });

        // Repository + UoW
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        // Encryption
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.Key));
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Keycloak Admin API — Bearer token otomatik ekleniyor
        services.AddTransient<KeycloakAdminTokenHandler>();
        services.AddHttpClient("KeycloakAdmin", client =>
        {
            var baseUrl = configuration["KeycloakSettings:BaseUrl"] ?? "http://localhost:8080";
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<KeycloakAdminTokenHandler>();

        services.AddScoped<IKeycloakService, KeycloakService>();

        return services;
    }
}
