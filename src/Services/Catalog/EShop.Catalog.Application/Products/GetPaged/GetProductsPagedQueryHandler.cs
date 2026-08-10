using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Application.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.Application.Products.GetPaged;

/// <summary>
/// Handler de listado paginado usando PROYECCIÓN directa (no carga agregados).
///
/// Flujo:
/// 1. Construye query base (filtro opcional por categoría).
/// 2. Cuenta el total ANTES de paginar (para calcular TotalPages).
/// 3. Aplica Skip/Take y proyecta directo a ProductDto con JOIN a Category.
/// 4. Todo en UNA sola ida a BD para el conteo + otra para los datos
///    (EF Core no puede combinar ambas en una query eficientemente).
/// </summary>
public sealed class GetProductsPagedQueryHandler
    : IQueryHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    private readonly ICatalogReadContext _readContext;

    public GetProductsPagedQueryHandler(ICatalogReadContext readContext)
    {
        _readContext = readContext;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsPagedQuery request,
        CancellationToken cancellationToken)
    {
        // Query base compuesta - todavía NO se ejecuta contra la BD (IQueryable es lazy).
        var query = _readContext.Products.AsQueryable();

        if (request.CategoryId is not null)
        {
            var categoryId = new Domain.Categories.CategoryId(request.CategoryId.Value);
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // CountAsync ejecuta un SELECT COUNT(*) - eficiente, no trae filas.
        var totalCount = await query.CountAsync(cancellationToken);

        // Proyección directa: JOIN a Category incluido en el SELECT, sin tracking,
        // sin cargar el agregado completo. Esto es lo más eficiente posible.
        var items = await query
            .OrderBy(p => p.Name) // Orden determinístico - CRÍTICO para paginación consistente
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                _readContext.Categories,
                product => product.CategoryId,
                category => category.Id,
                (product, category) => new ProductDto(
                    product.Id.Value,
                    product.Sku.Value,
                    product.Name,
                    product.Description,
                    product.Price.Amount,
                    product.Price.Currency,
                    category.Id.Value,
                    category.Name,
                    product.Status.ToString()))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductDto>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return result;
    }
}