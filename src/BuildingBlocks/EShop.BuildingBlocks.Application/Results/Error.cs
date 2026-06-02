using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.BuildingBlocks.Application.Results
{
    /// <summary>
    /// Representa un error de negocio estructurado.
    ///
    /// Usamos 'record' porque:
    /// 1. Es inmutable por default (los errores no deben mutarse).
    /// 2. Tiene value equality (dos errores con mismo Code/Description son iguales).
    /// 3. Sintaxis concisa y moderna.
    /// </summary>
    public record Error(string Code, string Description, ErrorType Type)
    {
        /// <summary>
        /// Error vacío para indicar "no hay error" (usado en Result.Success).
        /// Es 'static readonly' porque queremos UNA instancia compartida.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        /// <summary>
        /// Error específico cuando un valor nulo se recibe inesperadamente.
        /// </summary>
        public static readonly Error NullValue = new(
            "Error.NullValue",
            "A null value was provided.",
            ErrorType.Failure);

        // Factory methods estáticos para crear errores tipados.
        // Son más expresivos que llamar al constructor con un enum.

        public static Error NotFound(string code, string description)
            => new(code, description, ErrorType.NotFound);

        public static Error Validation(string code, string description)
            => new(code, description, ErrorType.Validation);

        public static Error Conflict(string code, string description)
            => new(code, description, ErrorType.Conflict);

        public static Error Unauthorized(string code, string description)
            => new(code, description, ErrorType.Unauthorized);

        public static Error Forbidden(string code, string description)
            => new(code, description, ErrorType.Forbidden);

        public static Error Failure(string code, string description)
            => new(code, description, ErrorType.Failure);
    }

    /// <summary>
    /// Tipos de error que mapean a códigos HTTP en la capa Web.
    /// El middleware de excepciones leerá este enum para decidir qué status code devolver.
    /// </summary>
    public enum ErrorType
    {
        Failure = 0,        // HTTP 500 - error técnico genérico
        Validation = 1,     // HTTP 400 - input inválido
        NotFound = 2,       // HTTP 404 - recurso no existe
        Conflict = 3,       // HTTP 409 - conflicto (ej: ya existe)
        Unauthorized = 4,   // HTTP 401 - no autenticado
        Forbidden = 5       // HTTP 403 - autenticado pero sin permisos
    }
}
