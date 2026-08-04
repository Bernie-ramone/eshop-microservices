using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.BuildingBlocks.Application.Abstractions;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.Products;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Application.Products.Create;

/// <summary>
/// Handler de CreateProductCommand.
///
/// Responsabilidades (y SOLO estas):
/// 1. Validar reglas de negocio que requieren BD (SKU único, categoría existe).
/// 2. Invocar el factory del dominio (Product.Create) para construir el agregado.
/// 3. Persistir vía repositorio + Unit of Work.
///
/// El handler NO valida formato (eso ya lo hizo el ValidationBehavior con FluentValidation).
/// El handler NO calcula precios ni aplica reglas complejas (eso vive en Product).
/// </summary>
public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que la categoría existe (regla de negocio que requiere BD).
        var categoryId = new CategoryId(request.CategoryId);
        var categoryExists = await _categoryRepository.ExistsAsync(categoryId, cancellationToken);

        if (!categoryExists)
            return Result.Failure<Guid>(ProductErrors.CategoryNotFound);

        // 2. Validar que el SKU es único (regla de negocio que requiere BD).
        var sku = Sku.Create(request.Sku);
        var skuExists = await _productRepository.ExistsWithSkuAsync(sku, cancellationToken);

        if (skuExists)
            return Result.Failure<Guid>(ProductErrors.SkuAlreadyExists);

        // 3. Construir el Value Object Money.
        // Si esto lanza DomainException, el GlobalExceptionHandler (Web BB) la capturará
        // y la traducirá a 400. Pero como YA validamos formato con FluentValidation,
        // este camino solo se activaría por un bug, no por input de usuario normal.
        var price = Money.Create(request.Price, request.Currency);

        // 4. Crear el agregado usando el FACTORY del dominio.
        // Aquí es donde Product.Create() dispara el ProductCreatedDomainEvent.
        var product = Product.Create(
            sku: sku,
            name: request.Name,
            price: price,
            categoryId: categoryId,
            description: request.Description);

        // 5. Registrar el agregado para persistencia (aún no toca BD).
        _productRepository.Add(product);

        // 6. Commit: aquí es donde EF Core hace INSERT y los interceptors
        // (Infrastructure BuildingBlock) entran en acción:
        //    - AuditableEntityInterceptor pone CreatedAtUtc.
        //    - PublishDomainEventsInterceptor publica ProductCreatedDomainEvent.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Devolvemos el Id. El operador implícito convierte Guid -> Result<Guid>.
        return product.Id.Value;
    }
}