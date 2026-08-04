using EShop.BuildingBlocks.Application.Abstractions.Messaging;

namespace EShop.Catalog.Application.Products.ChangePrice;

public sealed record ChangeProductPriceCommand(
    Guid ProductId,
    decimal NewPrice,
    string Currency) : ICommand;