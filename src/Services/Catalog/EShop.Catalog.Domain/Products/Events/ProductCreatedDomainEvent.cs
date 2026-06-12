using EShop.BuildingBlocks.Domain.Abstractions;
using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Domain.Products.Events;

/// <summary>
/// Domain Event disparado cuando se crea un nuevo Product.
///
/// Es un Domain Event (no Integration Event) porque vive dentro del bounded
/// context del Catalog. Un handler interno lo escuchará y lo traducirá a un
/// Integration Event si otros servicios necesitan enterarse.
///
/// Es 'record' porque:
/// - Inmutable (los eventos NO se mutan después de creados).
/// - Igualdad por valor automática.
/// - Sintaxis concisa.
/// </summary>
public sealed record ProductCreatedDomainEvent(
    ProductId ProductId,
    Sku Sku,
    string Name,
    Money Price,
    CategoryId CategoryId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}