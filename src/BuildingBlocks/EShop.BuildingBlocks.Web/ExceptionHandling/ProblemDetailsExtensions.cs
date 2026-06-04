using EShop.BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EShop.BuildingBlocks.Web.ExceptionHandling;

/// <summary>
/// Helpers para construir ProblemDetails (RFC 7807) a partir de errores
/// estructurados de nuestra capa Application.
///
/// La idea: centralizar el mapeo Error → ProblemDetails en UN solo lugar,
/// para que todos los servicios respondan errores con el mismo formato.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Base URL para los "type" links de ProblemDetails.
    /// En un proyecto real, apuntarías a docs públicas.
    /// </summary>
    private const string ProblemTypeBaseUrl = "https://docs.eshop.com/errors";

    /// <summary>
    /// Crea un ProblemDetails desde un Error de la capa Application.
    /// </summary>
    /// <param name="error">El error a mapear.</param>
    /// <param name="instance">URL donde ocurrió (típicamente HttpContext.Request.Path).</param>
    public static ProblemDetails ToProblemDetails(this Error error, string? instance = null)
    {
        // Mapeamos el ErrorType a su correspondiente HTTP status code.
        int statusCode = GetStatusCode(error.Type);

        return new ProblemDetails
        {
            // Type: URI que identifica el TIPO de error.
            // Ej: "https://docs.eshop.com/errors/not-found"
            Type = $"{ProblemTypeBaseUrl}/{error.Type.ToString().ToLowerInvariant()}",

            // Title: descripción genérica del tipo de error.
            // Mismo para todos los "not found", por ejemplo.
            Title = GetTitle(error.Type),

            // Status: código HTTP.
            Status = statusCode,

            // Detail: descripción específica de ESTA instancia.
            // Aquí va el mensaje del error concreto.
            Detail = error.Description,

            // Instance: dónde ocurrió.
            Instance = instance,

            // Extensions: campos adicionales no estándar pero permitidos.
            // Aquí guardamos el código interno para que el cliente pueda manejarlo.
            Extensions =
            {
                ["errorCode"] = error.Code
            }
        };
    }

    /// <summary>
    /// Mapeo central de ErrorType → HTTP status code.
    /// Mantén esto sincronizado con la convención del proyecto.
    /// </summary>
    public static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Títulos genéricos por tipo de error.
    /// </summary>
    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => "Resource not found",
        ErrorType.Validation => "Validation failed",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Failure => "An error occurred",
        _ => "An error occurred"
    };
}
