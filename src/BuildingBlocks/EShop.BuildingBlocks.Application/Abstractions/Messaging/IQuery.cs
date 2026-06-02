using EShop.BuildingBlocks.Application.Results;
using MediatR;

namespace EShop.BuildingBlocks.Application.Abstractions.Messaging
{
    /// <summary>
    /// Marker interface para queries (operaciones de LECTURA).
    ///
    /// Una query NO MODIFICA el estado, solo consulta datos.
    /// SIEMPRE devuelve un valor (Result&lt;TResponse&gt;).
    ///
    /// Ejemplo: GetProductByIdQuery, SearchProductsQuery
    /// </summary>
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
}
