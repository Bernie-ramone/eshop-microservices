namespace EShop.Catalog.Application.Abstractions;

/// <summary>
/// Abstracción para el sistema de caching del Catalog.
///
/// Implementada con Redis en Infrastructure (Subfase 2.3).
/// Application no sabe si es Redis, memoria, o cualquier otro proveedor.
///
/// Generic para reutilizar con cualquier DTO cacheable (ProductDto, CategoryDto, etc).
/// </summary>
public interface ICatalogCacheService
{
    /// <summary>
    /// Intenta obtener un valor cacheado. Devuelve null si no existe (cache miss).
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Guarda un valor en cache con expiración.
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invalida (elimina) una entrada de cache.
    /// Se usa cuando el dato subyacente cambia (ej: al cambiar precio de un producto).
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}