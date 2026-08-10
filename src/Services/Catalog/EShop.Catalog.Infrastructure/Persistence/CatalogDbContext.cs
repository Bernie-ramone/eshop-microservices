using System.Linq;
using EShop.BuildingBlocks.Application.Abstractions;
using EShop.BuildingBlocks.Infrastructure.Persistence.Interceptors;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.Products;

using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.Infrastructure.Persistence;

/// <summary>
/// DbContext del Catalog Service. Implementa IUnitOfWork (Application BuildingBlock)
/// simplemente heredando de DbContext, que ya tiene SaveChangesAsync con la firma correcta.
///
/// Registra los interceptors del Infrastructure BuildingBlock para que:
/// - AuditableEntityInterceptor: pueble CreatedAtUtc/UpdatedAtUtc automáticamente.
/// - PublishDomainEventsInterceptor: publique Domain Events después de SaveChanges.
/// </summary>
public sealed class CatalogDbContext : DbContext, IUnitOfWork, ICatalogReadContext
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    IQueryable<Product> ICatalogReadContext.Products => Products;
    IQueryable<Category> ICatalogReadContext.Categories => Categories;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplyConfigurationsFromAssembly busca TODAS las clases que implementan
        // IEntityTypeConfiguration<T> en este assembly y las aplica automáticamente.
        // Así no tenemos que registrar cada configuration manualmente aquí.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // Nota: los interceptors NO se registran aquí dentro del DbContext.
    // Se registran en el AddDbContext() de Program.cs vía DI, porque
    // PublishDomainEventsInterceptor necesita IPublisher (MediatR) inyectado,
    // y el DbContext no debería resolver dependencias de DI directamente.
    // Lo veremos en la Subfase 2.4.
}