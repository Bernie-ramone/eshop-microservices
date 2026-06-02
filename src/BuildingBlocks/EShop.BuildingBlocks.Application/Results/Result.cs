using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.BuildingBlocks.Application.Results
{
    /// <summary>
    /// Representa el resultado de una operación que puede fallar.
    ///
    /// La clave: el TIPO te dice si fue exitosa o no.
    /// Si IsSuccess es true, Error es Error.None.
    /// Si IsFailure es true, Error contiene el motivo.
    ///
    /// Nunca tendrás IsSuccess=true CON un Error real (lo garantizamos en el constructor).
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Constructor protegido para que NADIE pueda crear Results inválidos desde fuera.
        /// Solo se crean a través de los factory methods Success() y Failure().
        /// </summary>
        protected Result(bool isSuccess, Error error)
        {
            // Invariantes: validamos que el estado sea coherente.
            // Un Result no puede ser "exitoso pero con error" ni "fallido pero sin error".
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("A successful result cannot have an error.");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("A failed result must have an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        /// <summary>
        /// Crea un Result exitoso sin valor (típico para Commands que no devuelven datos).
        /// </summary>
        public static Result Success() => new(true, Error.None);

        /// <summary>
        /// Crea un Result fallido con el error indicado.
        /// </summary>
        public static Result Failure(Error error) => new(false, error);

        /// <summary>
        /// Crea un Result&lt;T&gt; exitoso con un valor.
        /// </summary>
        public static Result<T> Success<T>(T value) => new(value, true, Error.None);

        /// <summary>
        /// Crea un Result&lt;T&gt; fallido SIN valor (default(T)).
        /// </summary>
        public static Result<T> Failure<T>(Error error) => new(default, false, error);
    }
}
