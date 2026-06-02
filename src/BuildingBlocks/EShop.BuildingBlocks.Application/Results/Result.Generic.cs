using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EShop.BuildingBlocks.Application.Results
{
    /// <summary>
    /// Result con un valor de retorno tipado.
    /// Hereda de Result para reutilizar IsSuccess/IsFailure/Error.
    /// </summary>
    public class Result<TValue> : Result
    {
        // El valor SOLO existe si IsSuccess. Si IsFailure, _value es default(T).
        private readonly TValue? _value;

        /// <summary>
        /// Constructor interno: lo crea la clase base via factory methods.
        /// </summary>
        protected internal Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        /// <summary>
        /// Accede al valor. SOLO debe llamarse cuando IsSuccess es true.
        /// Si llamas .Value en un Result fallido, LANZAMOS excepción.
        /// Esto previene bugs donde alguien olvida verificar IsSuccess.
        /// </summary>
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

        /// <summary>
        /// Conversión IMPLÍCITA: te permite escribir 'return product' en un método
        /// que devuelve Result&lt;Product&gt;, sin tener que escribir 'return Result.Success(product)'.
        ///
        /// Si el valor es null, devuelve un Result fallido con Error.NullValue.
        /// </summary>
        public static implicit operator Result<TValue>(TValue? value)
            => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    }
}
