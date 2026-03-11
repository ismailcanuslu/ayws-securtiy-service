using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ayws.Security.Service.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationAssembly).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        // AutoMapper — assembly'deki tüm Profile sınıflarını tarar
        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        // FluentValidation pipeline behavior (validation before handler)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        return services;
    }
}
