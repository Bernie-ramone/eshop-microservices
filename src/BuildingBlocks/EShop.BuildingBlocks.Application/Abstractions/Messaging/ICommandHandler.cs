using EShop.BuildingBlocks.Application.Results;
using MediatR;

namespace EShop.BuildingBlocks.Application.Abstractions.Messaging
{
    /// <summary>
    /// Handler de un comando sin valor de retorno.
    /// Es el que CONTIENE la lógica de negocio del caso de uso.
    /// </summary>
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
    {
    }

    /// <summary>
    /// Handler de un comando con valor de retorno.
    /// </summary>
    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
    {
    }
}
