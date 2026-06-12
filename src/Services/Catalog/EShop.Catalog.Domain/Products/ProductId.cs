using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.Catalog.Domain.Products;

/// <summary>
/// Identificador strongly-typed para Product.
///
/// ¿Por qué readonly record struct?
/// - readonly: inmutable después de creación
/// - record: igualdad por valor automática (dos ProductId con mismo Guid son ==)
/// - struct: stack allocated, NUNCA puede ser null, más rápido
///
/// Esto previene bugs como pasar un CategoryId donde se espera un ProductId:
///   _repo.GetByIdAsync(categoryId)  // ❌ Compile error si tipos diferentes
/// </summary>
public readonly record struct ProductId(Guid Value)
{
    /// <summary>
    /// Factory para crear un nuevo ID. Más expresivo que 'new ProductId(Guid.NewGuid())'.
    /// </summary>
    public static ProductId New() => new(Guid.NewGuid());

    /// <summary>
    /// Override de ToString para que en logs aparezca solo el Guid, no "ProductId { Value = ... }".
    /// </summary>
    public override string ToString() => Value.ToString();
}