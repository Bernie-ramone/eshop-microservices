using EShop.BuildingBlocks.Application.Exceptions;
using EShop.BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EShop.BuildingBlocks.Web.ExceptionHandling;

/// <summary>
/// Captura TODAS las excepciones no manejadas y las traduce a ProblemDetails.
///
/// Cómo se integra en ASP.NET Core:
/// 1. Se registra en DI con services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()
/// 2. Se activa el middleware con app.UseExceptionHandler()
/// 3. ASP.NET Core invoca TryHandleAsync automáticamente ante cualquier excepción.
///
/// Mapeo:
///   - ValidationException → 400 con diccionario de errores
///   - DomainException → 400 con mensaje del dominio
///   - Cualquier otra Exception → 500 (genérico, sin filtrar detalles)
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invocado por ASP.NET Core cuando una excepción NO se maneja en el endpoint.
    /// </summary>
    /// <returns>
    /// true → manejé la excepción, no la propagues.
    /// false → no la manejé, pásala al siguiente handler o al default.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // SIEMPRE loggeamos la excepción.
        // LogError incluye automáticamente el stack trace.
        _logger.LogError(
            exception,
            "Unhandled exception occurred: {Message}",
            exception.Message);

        // Construimos el ProblemDetails según el TIPO de excepción.
        // Pattern matching de C# 8+ hace esto muy expresivo.
        ProblemDetails problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, httpContext),
            DomainException domainEx => CreateDomainProblemDetails(domainEx, httpContext),
            _ => CreateGenericProblemDetails(httpContext)
        };

        // Seteamos el status code en la response HTTP.
        // IMPORTANTE: esto se hace ANTES de escribir el body.
        httpContext.Response.StatusCode = problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        // El header oficial de ProblemDetails según RFC 7807.
        httpContext.Response.ContentType = "application/problem+json";

        // Serializamos como JSON y escribimos en la response.
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        // true = "manejé la excepción, NO la dejes propagar más"
        return true;
    }

    /// <summary>
    /// Construye ProblemDetails para ValidationException.
    /// Incluye el diccionario errors con detalles por campo.
    /// </summary>
    private static ValidationProblemDetails CreateValidationProblemDetails(
    ValidationException exception,
    HttpContext httpContext)
    {
        return new ValidationProblemDetails(exception.Errors.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value))
        {
            // ...
        };
    }

    /// <summary>
    /// Construye ProblemDetails para DomainException.
    /// Estos errores indican violaciones de reglas de negocio.
    /// </summary>
    private static ProblemDetails CreateDomainProblemDetails(
        DomainException exception,
        HttpContext httpContext)
    {
        return new ProblemDetails
        {
            Type = "https://docs.eshop.com/errors/domain-rule-violated",
            Title = "Business rule violation",
            Status = StatusCodes.Status400BadRequest,
            // Ojo: el mensaje del DomainException sí lo exponemos al cliente.
            // Está diseñado para ser legible por humanos (ej: "Cannot confirm cancelled order").
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };
    }

    /// <summary>
    /// ProblemDetails GENÉRICO para excepciones inesperadas.
    /// NUNCA expone detalles internos al cliente: stack trace, mensajes técnicos, etc.
    /// </summary>
    private static ProblemDetails CreateGenericProblemDetails(HttpContext httpContext)
    {
        return new ProblemDetails
        {
            Type = "https://docs.eshop.com/errors/internal-server-error",
            Title = "An error occurred",
            Status = StatusCodes.Status500InternalServerError,
            // OJO: NO ponemos exception.Message en Detail.
            // Filtraríamos info interna al atacante (rutas, stacks, nombres de BD).
            // Devolvemos algo genérico y trackeable.
            Detail = "An unexpected error occurred. Please contact support if the problem persists.",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                // El traceId es CRÍTICO para correlacionar el error en logs.
                // El cliente puede mandarlo a soporte, y ahí lo buscamos.
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
    }
}