using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Products;
using EShop.Catalog.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación concreta de IProductRepository usando EF Core.
///
/// Nota: los métodos aquí son DELIBERADAMENTE simples. No hay lógica de
/// negocio - solo acceso a datos. Las reglas viven en Product (dominio)
/// y en los Handlers (orquestación).
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        // SIN AsNoTracking: este método se usa en Commands que van a MUTAR
        // el agregado, así que necesitamos que EF Core trackee los cambios
        // para generar el UPDATE correcto en SaveChanges.
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithSkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        // AnyAsync genera un EXISTS() en SQL - mucho más eficiente que
        // traer la entidad completa solo para verificar existencia.
        return await _context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public void Add(Product product)
    {
        // Solo agrega al ChangeTracker. El INSERT real sucede en SaveChangesAsync.
        _context.Products.Add(product);
    }
}