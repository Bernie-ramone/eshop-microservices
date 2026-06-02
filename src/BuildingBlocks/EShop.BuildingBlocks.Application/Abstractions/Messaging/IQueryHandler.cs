using EShop.BuildingBlocks.Application.Results;
using MediatR;

namespace EShop.BuildingBlocks.Application.Abstractions.Messaging
{
    /// <summary>
    /// Handler de una query. SIEMPRE tiene un TResponse.
    /// </summary>
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
    {
    }
}
