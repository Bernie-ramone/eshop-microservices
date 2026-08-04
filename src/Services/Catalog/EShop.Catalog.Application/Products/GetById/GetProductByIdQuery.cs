using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.Catalog.Application.Products.Dtos;

namespace EShop.Catalog.Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;