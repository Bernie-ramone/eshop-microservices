using EShop.BuildingBlocks.Application.Results;

namespace EShop.Catalog.Application.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Category.NotFound",
        $"The category with ID '{id}' was not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Category.NameAlreadyExists",
        "A category with this name already exists.");
}