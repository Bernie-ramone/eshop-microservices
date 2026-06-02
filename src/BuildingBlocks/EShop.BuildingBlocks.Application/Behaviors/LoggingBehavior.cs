using MediatR;
using Microsoft.Extensions.Logging;


namespace EShop.BuildingBlocks.Application.Behaviors
{
    /// <summary>
    /// Behavior que loggea el inicio, fin y duración de cada request.
    ///
    /// Se ejecuta DESPUÉS de ValidationBehavior pero ANTES del handler.
    /// Si el handler lanza excepción, se loggea como error.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // nameof() devuelve el nombre del TIPO en tiempo de compilación,
            // pero como TRequest es genérico, usamos typeof(TRequest).Name.
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation(
                "Handling request {RequestName}",
                requestName);

            try
            {
                // Pasamos al siguiente behavior/handler.
                var response = await next();

                _logger.LogInformation(
                    "Handled request {RequestName} successfully",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                // Loggeamos con LogError porque incluye el stack trace.
                _logger.LogError(
                    ex,
                    "Request {RequestName} failed",
                    requestName);

                // Re-lanzamos: NO consumimos la excepción, solo la loggeamos.
                // El middleware web se encargará de convertirla a HTTP.
                throw;
            }
        }
    }
}
