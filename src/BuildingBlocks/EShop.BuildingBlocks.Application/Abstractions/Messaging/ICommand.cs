using EShop.BuildingBlocks.Application.Results;
using MediatR;

namespace EShop.BuildingBlocks.Application.Abstractions.Messaging
{
    /// <summary>
    /// Marker interface para comandos (operaciones de ESCRITURA).
    ///
    /// Un comando MODIFICA el estado del sistema.
    /// Devuelve un Result simple (éxito o fallo) sin datos de negocio.
    ///
    /// Ejemplo: CreateProductCommand, DeleteOrderCommand
    /// </summary>
    public interface ICommand : IRequest<Result>, IBaseCommand
    {
    }

    /// <summary>
    /// Comando que devuelve un valor además del Result.
    /// Útil cuando necesitas el ID del recurso creado, por ejemplo.
    ///
    /// Ejemplo: CreateProductCommand devuelve Result&lt;Guid&gt; con el ID del producto creado.
    /// </summary>
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
    {
    }

    /// <summary>
    /// Interface base SIN type parameter para permitir filtrado polimórfico.
    ///
    /// ¿Por qué necesitamos esto? Para que un behavior pueda decir
    /// "si el request es IBaseCommand, hago X" sin importar el TResponse.
    /// </summary>
    public interface IBaseCommand
    {
    }
}
