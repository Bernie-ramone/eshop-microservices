using EShop.BuildingBlocks.Application.Abstractions.Messaging;

namespace EShop.Catalog.Application.Products.Discontinue;

public sealed record DiscontinueProductCommand(Guid ProductId, string Reason) : ICommand;
