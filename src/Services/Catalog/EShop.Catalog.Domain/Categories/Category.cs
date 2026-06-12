using EShop.BuildingBlocks.Domain.Entities;
using EShop.BuildingBlocks.Domain.Exceptions;

namespace EShop.Catalog.Domain.Categories;

/// <summary>
/// Categoría de productos del catálogo (ej: "Bass Guitars", "Drums", "Amplifiers").
///
/// Category es una ENTITY (no AggregateRoot) porque:
/// - No tiene Domain Events propios.
/// - Su ciclo de vida está atado al contexto de Products.
/// - No se modifica independientemente desde el exterior - siempre vía Products.
///
/// Si en el futuro Category necesita lógica compleja (subcategorías, jerarquía,
/// permisos por categoría), la promoveríamos a AggregateRoot.
/// </summary>
public sealed class Category : Entity<CategoryId>
{
    private Category(CategoryId id, string name, string? description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    // Constructor para EF Core (hidratación desde BD).
#pragma warning disable CS8618
    private Category() { }
#pragma warning restore CS8618

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>
    /// Factory para crear una nueva categoría.
    /// Valida que Name no sea vacío y respete límites de longitud.
    /// </summary>
    public static Category Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        if (name.Length > 100)
            throw new DomainException($"Category name cannot exceed 100 characters. Got: {name.Length}");

        if (description?.Length > 500)
            throw new DomainException("Category description cannot exceed 500 characters.");

        return new Category(CategoryId.New(), name.Trim(), description?.Trim());
    }

    /// <summary>
    /// Cambia el nombre de la categoría. Re-valida las invariantes.
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Category name is required.");

        if (newName.Length > 100)
            throw new DomainException($"Category name cannot exceed 100 characters.");

        Name = newName.Trim();
    }

    /// <summary>
    /// Actualiza la descripción (puede ser null/vacía).
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        if (newDescription?.Length > 500)
            throw new DomainException("Category description cannot exceed 500 characters.");

        Description = newDescription?.Trim();
    }
}