using System;
using System.Collections.Generic;
using System.Text;
using EShop.BuildingBlocks.Application.Results;

namespace EShop.Catalog.Application.Products;

/// <summary>
/// Catálogo centralizado de errores de negocio relacionados a Product.
///
/// Por qué centralizamos aquí (y no en el Handler donde se usan):
/// 1. Reutilización: el mismo error puede aparecer en múltiples handlers.
/// 2. Testing: puedes assertar result.Error == ProductErrors.NotFound.
/// 3. Documentación: un vistazo a este archivo muestra TODOS los errores posibles del Product.
/// 4. Consistencia: el mismo código/mensaje siempre, no reescrito en cada lugar.
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Product.NotFound",
        $"The product with ID '{id}' was not found.");

    public static readonly Error SkuAlreadyExists = Error.Conflict(
        "Product.SkuAlreadyExists",
        "A product with this SKU already exists.");

    public static readonly Error CategoryNotFound = Error.NotFound(
        "Product.CategoryNotFound",
        "The specified category does not exist.");

    public static readonly Error InvalidPriceCurrency = Error.Validation(
        "Product.InvalidPriceCurrency",
        "The price currency does not match the product's current currency.");

    public static readonly Error AlreadyDiscontinued = Error.Conflict(
        "Product.AlreadyDiscontinued",
        "This product is already discontinued.");
}