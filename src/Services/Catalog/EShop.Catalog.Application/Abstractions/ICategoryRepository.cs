using EShop.Catalog.Domain.Categories;

namespace EShop.Catalog.Application.Abstractions;

/// <summary>
/// Abstracción del repositorio de Categories.
/// Más simple que IProductRepository porque Category es una Entity más liviana.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica existencia sin cargar la entidad completa - más eficiente
    /// para validaciones (ej: "¿existe esta categoría antes de crear un producto?").
    /// </summary>
    Task<bool> ExistsAsync(CategoryId id, CancellationToken cancellationToken = default);

    void Add(Category category);
}
