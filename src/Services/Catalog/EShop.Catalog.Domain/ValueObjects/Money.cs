using EShop.BuildingBlocks.Domain.Exceptions;
using EShop.BuildingBlocks.Domain.ValueObjects;

namespace EShop.Catalog.Domain.ValueObjects;


/// <summary>
/// Value Object que representa una cantidad monetaria con su moneda.
///
/// Por qué Money y no decimal:
/// 1. Un decimal sin currency es ambiguo: ¿100 USD? ¿100 MXN? ¿100 JPY?
/// 2. Encapsula reglas de validación (no puede ser negativo).
/// 3. En el futuro, agregarás operaciones (Add, Subtract) que validen la misma moneda.
///
/// Es 'sealed' porque no esperamos que nadie lo extienda. Esto permite
/// optimizaciones del compilador y comunica intención: "es completo, no toques".
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Constructor PRIVADO para que solo nuestros factories lo invoquen.
    /// Forza el uso de Money.Create() o Money.Zero(), que validan invariantes.
    /// </summary>
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Constructor sin parámetros REQUERIDO por EF Core para hidratar el VO desde la BD.
    /// Como es 'private', nadie del dominio puede usarlo directamente.
    /// EF Core lo invoca por reflection.
    /// </summary>
#pragma warning disable CS8618 // EF Core poblará las propiedades.
    private Money() { }
#pragma warning restore CS8618

    /// <summary>
    /// El monto. Decimal (NUNCA float/double para dinero - errores de precisión).
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Código ISO 4217 de la moneda (ej: "USD", "MXN", "EUR").
    /// Lo guardamos como string por simplicidad; en Ordering haremos un Currency VO completo.
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Factory para crear instancias válidas de Money.
    ///
    /// Valida:
    /// - Amount NO puede ser negativo (precios negativos no tienen sentido en Catalog).
    /// - Currency NO puede ser vacío y debe tener exactamente 3 caracteres (ISO 4217).
    ///
    /// Lanza DomainException si las reglas se violan - eso indica que el código que
    /// invoca está intentando crear estado inválido, lo cual es un bug del dominio.
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException($"Money amount cannot be negative. Got: {amount}");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency code is required.");

        // ISO 4217 son exactamente 3 letras (USD, MXN, EUR, JPY, etc).
        if (currency.Length != 3)
            throw new DomainException(
                $"Currency must be a 3-letter ISO 4217 code. Got: '{currency}'");

        // ToUpperInvariant para normalizar - guardamos siempre en mayúsculas.
        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Factory de conveniencia para crear Money con valor cero.
    /// Útil para inicializar campos.
    /// </summary>
    public static Money Zero(string currency) => Create(0, currency);

    /// <summary>
    /// Implementación requerida por la clase base ValueObject.
    /// Define qué propiedades determinan la igualdad: dos Money son iguales
    /// SI Y SOLO SI tienen el mismo Amount Y la misma Currency.
    /// </summary>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Override de ToString para logs legibles: "100.00 USD" en vez de "Money { ... }".
    /// </summary>
    public override string ToString() => $"{Amount:F2} {Currency}";
}

