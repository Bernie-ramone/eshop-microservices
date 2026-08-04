using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.Catalog.Application.Products.Dtos;

namespace EShop.Catalog.Application.Products.GetPaged;

/// <summary>
/// Query para listar productos paginados, con filtro opcional por categoría.
///
/// PageNumber y PageSize con valores default sensatos evitan que un cliente
/// pida "page 0" o "pageSize 10000" y rompa el servidor.
/// </summary>
public sealed record GetProductsPagedQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? CategoryId = null) : IQuery<PagedResult<ProductDto>>;
