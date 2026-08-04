using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.BuildingBlocks.Application.Abstractions;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Products;

namespace EShop.Catalog.Application.Products.Discontinue;

public sealed class DiscontinueProductCommandHandler : ICommandHandler<DiscontinueProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogCacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public DiscontinueProductCommandHandler(
        IProductRepository productRepository,
        ICatalogCacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DiscontinueProductCommand request, CancellationToken cancellationToken)
    {
        var productId = new ProductId(request.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.NotFound(request.ProductId));

        // product.Discontinue() lanza DomainException si ya estaba discontinuado.
        // Preferimos dejar que el DOMINIO valide esto (es SU regla), no el handler.
        product.Discontinue(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"product:{product.Id.Value}", cancellationToken);

        return Result.Success();
    }
}