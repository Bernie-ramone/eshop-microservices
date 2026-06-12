using EShop.BuildingBlocks.Domain.Abstractions;

namespace EShop.Catalog.Domain.Products.Events;

/// <summary>
/// Disparado cuando un Product se descontinúa.
///
/// Otros servicios reaccionan: Basket remueve el producto de carritos activos,
/// Inventory marca el stock como histórico, Search lo excluye de resultados.
/// </summary>
public sealed record ProductDiscontinuedDomainEvent(
    ProductId ProductId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}