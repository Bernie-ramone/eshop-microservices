using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Results;
using EShop.Catalog.Application.Products.Dtos;

namespace EShop.Catalog.Application.Products.GetPaged;

/// <summary>
/// Handler de listado paginado.
///
/// IMPORTANTE: la implementación REAL de este handler depende de acceso
/// directo al DbContext para proyecciones eficientes (Select a DTO con JOIN).
/// Como el DbContext vive en Infrastructure (Subfase 2.3), este handler
/// dependerá de una abstracción de "read model" que definiremos ahí.
///
/// Por ahora, dejamos la FORMA del handler (la interface que cumple) para
/// que quede claro el contrato. La implementación completa con EF Core
/// Projections la completamos en 2.3.
/// </summary>
public sealed class GetProductsPagedQueryHandler
    : IQueryHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    // La implementación completa (con IApplicationDbContext o similar)
    // se construye en la Subfase 2.3, donde SÍ tenemos el DbContext real.
    // Aquí documentamos la INTENCIÓN y el contrato.

    public Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsPagedQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Se completa en Subfase 2.3 con acceso directo a CatalogDbContext para proyecciones eficientes.");
    }
}
