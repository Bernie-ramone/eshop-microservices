using EShop.BuildingBlocks.Application.Abstractions;
using EShop.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Infrastructure.Caching;
using EShop.Catalog.Infrastructure.Persistence;
using EShop.Catalog.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace EShop.Catalog.Infrastructure;

/// <summary>
/// Punto único de registro de todos los servicios de Infrastructure.
///
/// Patrón "Extension Method de Composición": Program.cs (Subfase 2.4) solo
/// llamará a services.AddCatalogInfrastructure(configuration) - una línea,
/// en vez de 15 líneas de registros individuales ensuciando Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ============================================================
        // INTERCEPTORS - se registran como servicios para que EF Core
        // los resuelva vía DI al construir el DbContext.
        // ============================================================
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddScoped<PublishDomainEventsInterceptor>();
        // Scoped porque necesita IPublisher, que a su vez depende de
        // servicios scoped de MediatR.

        // ============================================================
        // DbContext con SQL Server + interceptors registrados
        // ============================================================
        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("CatalogDb")
                ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found.");

            options.UseSqlServer(connectionString);

            // Resolvemos los interceptors del container de DI en vez de "new"-earlos,
            // así PublishDomainEventsInterceptor recibe su IPublisher inyectado correctamente.
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<PublishDomainEventsInterceptor>());
        });

        // ============================================================
        // Registro de abstracciones -> implementaciones
        // ============================================================
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<ICatalogReadContext>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ============================================================
        // Redis Cache (preparado, se activa cuando tengas el contenedor corriendo)
        // ============================================================
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? "localhost:6379"; // Default para desarrollo local
            options.InstanceName = "EShop.Catalog:";
        });
        services.AddScoped<ICatalogCacheService, RedisCatalogCacheService>();

        return services;
    }
}