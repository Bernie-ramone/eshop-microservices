using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;


namespace EShop.BuildingBlocks.Web.Endpoints;

/// <summary>
/// Extension methods para descubrir y registrar endpoints IEndpoint
/// por reflection en cada assembly del servicio.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Registra todas las clases que implementan IEndpoint en el DI container.
    ///
    /// Por qué TRANSIENT y no Singleton:
    /// Los endpoints se invocan una sola vez al arrancar la app para registrarse.
    /// Pero como cada uno puede tener dependencias inyectadas en su método
    /// MapEndpoint (rara vez, pero pasa), usar transient es lo más seguro.
    /// </summary>
    /// <param name="services">El IServiceCollection del builder.</param>
    /// <param name="assembly">
    /// El assembly donde buscar IEndpoints. Típicamente typeof(Program).Assembly
    /// del servicio que llama.
    /// </param>
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Buscamos todos los tipos concretos del assembly que implementen IEndpoint.
        // Filtros:
        //   - IsAbstract: false  → no queremos clases abstractas
        //   - IsInterface: false → no queremos la interface misma
        //   - IsAssignableTo: true → queremos los que implementan IEndpoint
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        // TryAddEnumerable evita registros duplicados si por alguna razón
        // se llama dos veces al método.
        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    /// <summary>
    /// Invoca MapEndpoint() en TODAS las clases IEndpoint registradas.
    ///
    /// Se llama en Program.cs DESPUÉS de builder.Build() y los UseXxx middlewares.
    /// </summary>
    /// <param name="app">El WebApplication.</param>
    /// <param name="routeGroupBuilder">
    /// Opcional: un grupo de rutas para prefijo común (ej: "/api/v1").
    /// Si es null, registra en el root del app.
    /// </param>
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        // Resolvemos TODAS las instancias de IEndpoint del container.
        // Esto funciona gracias a TryAddEnumerable de arriba.
        IEnumerable<IEndpoint> endpoints = app.Services
            .GetRequiredService<IEnumerable<IEndpoint>>();

        // builder puede ser el WebApplication directamente, o un grupo (/api/v1).
        IEndpointRouteBuilder builder =
            routeGroupBuilder is null ? app : routeGroupBuilder;

        // Cada endpoint se registra a sí mismo.
        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
