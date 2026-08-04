using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Application.Products.Dtos;
using EShop.Catalog.Domain.Products;

namespace EShop.Catalog.Application.Products.GetById;

/// <summary>
/// Handler que implementa el patrón CACHE-ASIDE:
///
/// 1. Pregunta al cache primero.
/// 2. Si HIT (existe): devuelve directo, NUNCA toca la BD.
/// 3. Si MISS (no existe): consulta BD, mapea a DTO, GUARDA en cache, devuelve.
///
/// Este patrón es ideal para Catalog porque los productos cambian poco
/// comparado con la frecuencia de lectura (read-heavy service).
/// </summary>
public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    // TTL del cache: 5 minutos. Trade-off entre frescura de datos y carga en BD.
    // Si el precio cambia, ChangeProductPriceCommandHandler invalida el cache
    // INMEDIATAMENTE, así que no dependemos solo del TTL para consistencia.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IProductRepository _productRepository;
    private readonly ICatalogCacheService _cacheService;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICatalogCacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.ProductId}";

        // PASO 1: Cache-aside - intenta leer del cache primero.
        var cached = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached; // Cache HIT - no tocamos la BD.
        }

        // PASO 2: Cache MISS - consulta la BD.
        var productId = new ProductId(request.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDto>(ProductErrors.NotFound(request.ProductId));

        // PASO 3: Mapear a DTO.
        // Nota: aquí mapeamos MANUALMENTE en vez de Mapster porque necesitamos
        // el nombre de la categoría, que requeriría un JOIN adicional. Por eso,
        // en la query REAL con EF Core (Subfase 2.3), haremos una PROYECCIÓN directa
        // que trae Category.Name sin cargar el agregado completo. Por ahora,
        // ilustramos el mapeo conceptual:
        var dto = MapToDto(product, categoryName: "placeholder"); // Se resuelve en 2.3

        // PASO 4: Rellenar el cache para la próxima petición.
        await _cacheService.SetAsync(cacheKey, dto, CacheDuration, cancellationToken);

        return dto;
    }

    private static ProductDto MapToDto(Product product, string categoryName) => new(
        Id: product.Id.Value,
        Sku: product.Sku.Value,
        Name: product.Name,
        Description: product.Description,
        PriceAmount: product.Price.Amount,
        PriceCurrency: product.Price.Currency,
        CategoryId: product.CategoryId.Value,
        CategoryName: categoryName,
        Status: product.Status.ToString());
}
