using EShop.BuildingBlocks.Domain.Abstractions;
using EShop.Catalog.Domain.ValueObjects;

namespace EShop.Catalog.Domain.Products.Events;

/// <summary>
/// Disparado cuando el precio de un Product cambia.
///
/// Lleva tanto el OldPrice como el NewPrice para que los consumidores
/// puedan reaccionar contextualmente (ej: Basket actualiza precios en
/// carritos activos, Inventory recalcula valoraciones).
/// </summary>
public sealed record ProductPriceChangedDomainEvent(
    ProductId ProductId,
    Money OldPrice,
    Money NewPrice) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}