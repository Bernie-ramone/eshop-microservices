using EShop.Catalog.Domain.Products;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Application.Abstractions;

/// <summary>
/// Abstracción del repositorio de Products.
///
/// Implementada en Infrastructure con EF Core (Subfase 2.3).
/// Application depende de ESTA interface, nunca de EF Core directamente.
///
/// Nota: NO incluimos métodos de paginación/búsqueda compleja aquí.
/// Esos van en Queries dedicadas que usan el DbContext directamente
/// para proyecciones optimizadas (ver GetProductsPagedQueryHandler).
/// Este repositorio es solo para operaciones de escritura y lookups simples.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Busca un producto por su Id. Devuelve null si no existe.
    /// Usado en Commands que necesitan cargar el agregado completo para mutarlo.
    /// </summary>
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un producto con el SKU dado.
    /// Usado para validar unicidad antes de crear un producto nuevo.
    /// </summary>
    Task<bool> ExistsWithSkuAsync(Sku sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo producto al contexto. NO hace commit -
    /// el commit sucede vía IUnitOfWork.SaveChangesAsync() en el handler.
    /// </summary>
    void Add(Product product);
}