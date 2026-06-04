using EShop.BuildingBlocks.Application.Results;
using EShop.BuildingBlocks.Web.ExceptionHandling;
using Microsoft.AspNetCore.Http;

namespace EShop.BuildingBlocks.Web.Results;

/// <summary>
/// Extensions para mapear Result/Result&lt;T&gt; a IResult HTTP.
///
/// Pone toda la lógica de "Result a HTTP" en UN solo lugar, así
/// los endpoints quedan limpios y consistentes.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Mapea un Result&lt;T&gt; a IResult HTTP.
    /// - Si IsSuccess: 200 OK con el valor.
    /// - Si IsFailure: status code según el ErrorType + ProblemDetails.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok(result.Value);
        }

        return ToProblemResult(result.Error, httpContext);
    }

    /// <summary>
    /// Mapea un Result (sin valor) a IResult HTTP.
    /// - Si IsSuccess: 204 No Content (típico para commands sin response).
    /// - Si IsFailure: status code + ProblemDetails.
    /// </summary>
    public static IResult ToHttpResult(this Result result, HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.NoContent();
        }

        return ToProblemResult(result.Error, httpContext);
    }

    /// <summary>
    /// Variante para POST que crea recursos.
    /// - Si IsSuccess: 201 Created con header Location.
    /// - Si IsFailure: igual que las otras.
    /// </summary>
    public static IResult ToCreatedResult<T>(
        this Result<T> result,
        string locationUri,
        HttpContext? httpContext = null)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Created(locationUri, result.Value);
        }

        return ToProblemResult(result.Error, httpContext);
    }

    /// <summary>
    /// Helper privado: construye un IResult con ProblemDetails desde un Error.
    /// </summary>
    private static IResult ToProblemResult(Error error, HttpContext? httpContext)
    {
        string? instance = httpContext?.Request.Path;
        var problemDetails = error.ToProblemDetails(instance);

        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }
}