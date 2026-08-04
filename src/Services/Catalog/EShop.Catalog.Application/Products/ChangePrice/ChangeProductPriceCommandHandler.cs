using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.BuildingBlocks.Application.Abstractions;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Products;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Application.Products.ChangePrice;


/// <summary>
/// Handler que cambia el precio de un producto existente.
///
/// Este handler es un buen ejemplo de "orquestación fina":
/// - Carga el agregado.
/// - Delega la REGLA DE NEGOCIO al método del dominio (product.ChangePrice).
/// - Persiste.
///
/// El handler NO decide SI el cambio de precio es válido - eso lo decide
/// Product.ChangePrice() internamente (currency debe coincidir, no debe estar
/// discontinuado, etc). El handler solo ORQUESTA el caso de uso.
/// </summary>
public sealed class ChangeProductPriceCommandHandler : ICommandHandler<ChangeProductPriceCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProductPriceCommandHandler(
        IProductRepository productRepository,
        ICatalogCacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeProductPriceCommand request, CancellationToken cancellationToken)
    {
        var productId = new ProductId(request.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.NotFound(request.ProductId));

        // Aquí es donde el DOMINIO decide si el cambio es válido.
        // Si Product.ChangePrice lanza DomainException (ej: producto discontinuado,
        // o currency no coincide), el GlobalExceptionHandler la traduce a 400.
        var newPrice = Money.Create(request.NewPrice, request.Currency);
        product.ChangePrice(newPrice);

        // SaveChanges dispara: auditoría + ProductPriceChangedDomainEvent
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidamos el cache DESPUÉS de persistir exitosamente.
        // Si invalidáramos ANTES y el SaveChanges fallara, tendríamos
        // cache inconsistente con la BD.
        await _cacheService.RemoveAsync($"product:{product.Id.Value}", cancellationToken);

        return Result.Success();
    }
}