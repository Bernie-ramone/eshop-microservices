using EShop.BuildingBlocks.Application.Abstractions.Messaging;
using EShop.BuildingBlocks.Application.Exceptions;
using FluentValidation;
using MediatR;

namespace EShop.BuildingBlocks.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior que ejecuta TODOS los validadores registrados para un Command.
    ///
    /// Solo aplica a IBaseCommand (no a queries) porque las queries típicamente
    /// no necesitan validación compleja: sus parámetros son IDs o filtros.
    ///
    /// Se ejecuta ANTES del handler. Si hay errores, lanza ValidationException
    /// y el handler nunca corre.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class, IBaseCommand
    {
        // FluentValidation registra validadores como IEnumerable<IValidator<T>>.
        // Puede haber 0, 1, o varios validadores para el mismo request.
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Si no hay validadores registrados, simplemente pasa al siguiente behavior/handler.
            if (!_validators.Any())
            {
                return await next();
            }

            // ValidationContext es el "paquete" que FluentValidation espera.
            var context = new ValidationContext<TRequest>(request);

            // Ejecutamos TODOS los validadores en paralelo (Task.WhenAll).
            // Cada uno devuelve un ValidationResult con sus errores.
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Aplanamos todos los errores en una sola lista.
            // SelectMany convierte IEnumerable<IEnumerable<Failure>> en IEnumerable<Failure>.
            var failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            // Si hay AL MENOS un error, lanzamos.
            if (failures.Count > 0)
            {
                throw new Exceptions.ValidationException(failures);
            }

            // Todo OK: pasamos al siguiente eslabón del pipeline.
            return await next();
        }
    }
}
