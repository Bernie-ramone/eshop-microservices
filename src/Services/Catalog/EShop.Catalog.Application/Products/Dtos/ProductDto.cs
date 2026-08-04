namespace EShop.Catalog.Application.Products.Dtos;

/// <summary>
/// DTO (Data Transfer Object) de Product para exponer al exterior (HTTP, cache).
///
/// Por qué NO devolvemos el AggregateRoot Product directamente:
/// 1. Encapsulación: el dominio no debe filtrarse a la capa HTTP.
/// 2. Serialización: Product tiene Value Objects (Money, Sku) que no serializan
///    limpiamente a JSON sin configuración extra.
/// 3. Versionado independiente: puedo cambiar el DTO sin tocar el dominio.
/// 4. Seguridad: controlo EXACTAMENTE qué campos se exponen.
///
/// Es un 'record' porque es inmutable y solo transporta datos (sin comportamiento).
/// </summary>
public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal PriceAmount,
    string PriceCurrency,
    Guid CategoryId,
    string CategoryName,
    string Status);