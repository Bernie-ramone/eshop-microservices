using EShop.BuildingBlocks.Domain.Exceptions;
using EShop.BuildingBlocks.Domain.ValueObjects;

using System.Text.RegularExpressions;

namespace EShop.Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object para el Stock Keeping Unit (SKU) de un producto.
///
/// El SKU es el identificador COMERCIAL (no técnico) de un producto.
/// Ejemplos: "BSS-MM-RAY34-BLK", "GTR-FND-STRAT-WHT"
///
/// Reglas de negocio:
/// - Solo letras mayúsculas, dígitos y guiones medios.
/// - Mínimo 3 caracteres, máximo 50.
/// - No puede empezar ni terminar con guion.
///
/// Es un VO porque dos SKUs con el mismo valor son el mismo SKU,
/// independientemente de "qué instancia" sean.
/// </summary>
public sealed partial class Sku : ValueObject
{
    /// <summary>
    /// Regex compilada para validar el formato del SKU.
    /// Usamos source generator (GeneratedRegex) para performance:
    /// la regex se compila en build time, no en runtime.
    /// </summary>
    [GeneratedRegex(@"^[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SkuPattern();

    private const int MinLength = 3;
    private const int MaxLength = 50;

    private Sku(string value) => Value = value;

#pragma warning disable CS8618
    private Sku() { }
#pragma warning restore CS8618

    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Factory que valida el formato y normaliza a mayúsculas.
    /// </summary>
    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SKU cannot be empty.");

        // Normalizamos a mayúsculas antes de validar.
        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length < MinLength || normalized.Length > MaxLength)
            throw new DomainException(
                $"SKU must be between {MinLength} and {MaxLength} characters. Got: {normalized.Length}");

        if (!SkuPattern().IsMatch(normalized))
            throw new DomainException(
                $"SKU has invalid format. Must be uppercase alphanumeric with optional hyphens. Got: '{normalized}'");

        return new Sku(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Conversión IMPLÍCITA a string para conveniencia.
    /// Ejemplo: string skuStr = product.Sku;  // sin necesidad de .Value
    /// </summary>
    public static implicit operator string(Sku sku) => sku.Value;
}