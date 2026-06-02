using FluentValidation.Results;

namespace EShop.BuildingBlocks.Application.Exceptions
{
    /// <summary>
    /// Excepción específica para fallos de validación.
    /// Contiene la lista de errores que el middleware web traducirá a HTTP 400.
    ///
    /// ¿Por qué excepción y no Result? Porque las validaciones suceden en un
    /// pipeline behavior ANTES del handler. El handler nunca se ejecuta si la
    /// validación falla. La excepción es la forma de "salir corto" del pipeline.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("One or more validation failures occurred.")
        {
            Errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());
        }

        /// <summary>
        /// Diccionario: clave = propiedad, valor = lista de errores en esa propiedad.
        /// Estructura compatible con ProblemDetails RFC 7807.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
