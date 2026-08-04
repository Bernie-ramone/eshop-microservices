using EShop.BuildingBlocks.Application.Abstractions.Messaging;

namespace EShop.Catalog.Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name, string? Description) : ICommand<Guid>;
