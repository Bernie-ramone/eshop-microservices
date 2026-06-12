namespace EShop.Catalog.Domain.Categories;

/// <summary>
/// Identificador strongly-typed para Category.
/// Misma estructura que ProductId, garantizando type-safety entre tipos.
/// </summary>
public readonly record struct CategoryId(Guid Value)
{
    public static CategoryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}