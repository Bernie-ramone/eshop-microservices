using EShop.BuildingBlocks.Domain.Entities;
using EShop.BuildingBlocks.Domain.Exceptions;
using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.Products.Events;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Domain.Products;

/// <summary>
/// AggregateRoot que representa un Product del catálogo.
///
/// Como AggregateRoot:
/// - Es el ÚNICO punto de entrada para modificaciones desde el exterior.
/// - Genera Domain Events ante cambios significativos.
/// - Garantiza las invariantes del negocio (precio no negativo, status válido, etc).
///
/// Los métodos públicos representan los CASOS DE USO del dominio:
/// - Create: crear un nuevo producto
/// - ChangePrice: cambiar el precio
/// - Rename: cambiar el nombre
/// - Discontinue: descontinuar
/// - UpdateDescription: actualizar descripción
///
/// Notar que NO hay setters públicos - toda mutación pasa por un método con nombre.
/// Esto se llama "tell, don't ask": le DICES al objeto qué hacer, no le SACAS datos
/// para hacerlo desde fuera.
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    /// <summary>
    /// Constructor privado: solo Product.Create() puede instanciar.
    /// Esto fuerza el uso del factory que valida invariantes.
    /// </summary>
    private Product(
        ProductId id,
        Sku sku,
        string name,
        string? description,
        Money price,
        CategoryId categoryId,
        ProductStatus status)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        Status = status;
    }

    /// <summary>
    /// Constructor sin parámetros para EF Core (hidratación desde BD).
    /// Es 'private' para que solo EF Core lo use por reflection.
    /// </summary>
#pragma warning disable CS8618
    private Product() { }
#pragma warning restore CS8618

    // PROPIEDADES con private set: nadie de fuera puede modificarlas directamente.
    // Toda mutación pasa por métodos con nombre semántico.

    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; }
    public ProductStatus Status { get; private set; }

    // ============================================================
    //  FACTORY METHODS
    // ============================================================

    /// <summary>
    /// Crea un nuevo Product en estado Active.
    /// Valida todas las invariantes y dispara ProductCreatedDomainEvent.
    /// </summary>
    public static Product Create(
        Sku sku,
        string name,
        Money price,
        CategoryId categoryId,
        string? description = null)
    {
        // Validaciones de dominio.
        // Lanzamos DomainException porque son violaciones de REGLAS DE NEGOCIO,
        // no errores técnicos.
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        if (name.Length > 200)
            throw new DomainException($"Product name cannot exceed 200 characters. Got: {name.Length}");

        if (description?.Length > 2000)
            throw new DomainException("Product description cannot exceed 2000 characters.");

        // Crear la instancia.
        var product = new Product(
            id: ProductId.New(),
            sku: sku,
            name: name.Trim(),
            description: description?.Trim(),
            price: price,
            categoryId: categoryId,
            status: ProductStatus.Active);

        // Disparar el Domain Event. Este evento será procesado por:
        // - El PublishDomainEventsInterceptor (Infrastructure BB) tras SaveChanges.
        // - Un handler que lo traducirá a ProductCreatedIntegrationEvent.
        // - El Outbox lo persistirá en la misma transacción.
        product.RaiseDomainEvent(new ProductCreatedDomainEvent(
            product.Id,
            product.Sku,
            product.Name,
            product.Price,
            product.CategoryId));

        return product;
    }

    // ============================================================
    //  BEHAVIORS (casos de uso del dominio)
    // ============================================================

    /// <summary>
    /// Cambia el precio del producto.
    ///
    /// Reglas:
    /// - El producto debe estar Active. Productos descontinuados no cambian de precio.
    /// - El nuevo precio debe usar la misma moneda que el actual.
    /// - Si el nuevo precio es igual al actual, NO se dispara evento (no es un cambio real).
    /// </summary>
    public void ChangePrice(Money newPrice)
    {
        if (Status == ProductStatus.Discontinued)
            throw new DomainException("Cannot change price of a discontinued product.");

        if (newPrice.Currency != Price.Currency)
            throw new DomainException(
                $"Cannot change price currency. Product is in {Price.Currency}, new price is in {newPrice.Currency}.");

        // Si el precio NO cambia, no hacemos nada. NO disparamos evento espurio.
        if (newPrice == Price)
            return;

        var oldPrice = Price;
        Price = newPrice;

        RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice));
    }

    /// <summary>
    /// Renombra el producto. No dispara evento porque es un cambio "cosmético"
    /// que otros servicios típicamente no necesitan saber.
    /// (Si fuera relevante, agregaríamos ProductRenamedDomainEvent.)
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Product name is required.");

        if (newName.Length > 200)
            throw new DomainException("Product name cannot exceed 200 characters.");

        Name = newName.Trim();
    }

    /// <summary>
    /// Actualiza la descripción del producto.
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        if (newDescription?.Length > 2000)
            throw new DomainException("Product description cannot exceed 2000 characters.");

        Description = newDescription?.Trim();
    }

    /// <summary>
    /// Descontinúa el producto.
    ///
    /// Estado terminal: una vez Discontinued, NO se puede volver a Active.
    /// (Si en el futuro se requiere "reactivar", agregaríamos un método Reactivate()
    /// con su propia regla de negocio y evento.)
    /// </summary>
    public void Discontinue(string reason)
    {
        if (Status == ProductStatus.Discontinued)
            throw new DomainException("Product is already discontinued.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reason is required to discontinue a product.");

        Status = ProductStatus.Discontinued;

        RaiseDomainEvent(new ProductDiscontinuedDomainEvent(Id, reason));
    }
}