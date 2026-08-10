using System.Text.Json;
using EShop.Catalog.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Catalog.Infrastructure.Caching;

/// <summary>
/// Implementación de ICatalogCacheService usando Redis vía IDistributedCache.
///
/// IDistributedCache es la abstracción de Microsoft para caching distribuido.
/// Redis es el PROVEEDOR concreto (configurado en Program.cs, Subfase 2.4),
/// pero este código no lo sabe explícitamente - solo usa IDistributedCache.
///
/// Serializamos a JSON porque IDistributedCache solo maneja byte[].
/// </summary>
public sealed class RedisCatalogCacheService : ICatalogCacheService
{
    private readonly IDistributedCache _cache;

    // Opciones de serialización consistentes (case-insensitive para JSON de entrada,
    // aunque aquí solo serializamos DTOs internos así que es más por consistencia).
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCatalogCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        // GetAsync devuelve null si la key no existe (cache miss) - comportamiento nativo.
        var bytes = await _cache.GetAsync(key, cancellationToken);

        if (bytes is null)
            return null;

        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            // Absolute expiration: expira SIEMPRE después de este tiempo,
            // sin importar cuántas veces se lea (a diferencia de sliding expiration
            // que renovaría el TTL en cada lectura).
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
