namespace EShop.Catalog.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description);
