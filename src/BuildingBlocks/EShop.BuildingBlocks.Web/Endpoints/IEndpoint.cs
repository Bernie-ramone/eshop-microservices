using Microsoft.AspNetCore.Routing;

namespace EShop.BuildingBlocks.Web.Endpoints;

/// <summary>
/// Contrato para clases que registran endpoints de Minimal APIs.
///
/// Cada feature tiene su propia clase que implementa esta interface:
///   - GetProductByIdEndpoint : IEndpoint
///   - CreateProductEndpoint : IEndpoint
///   - DeleteProductEndpoint : IEndpoint
///
/// El método MapEndpoints() las descubre todas por reflection y las registra.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Registra el endpoint en el router de ASP.NET Core.
    /// La implementación debe llamar app.MapGet/MapPost/etc.
    /// </summary>
    void MapEndpoint(IEndpointRouteBuilder app);
}
