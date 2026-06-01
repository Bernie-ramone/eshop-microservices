namespace EShop.BuildingBlocks.Domain.Abstractions
{

    /// <summary>
    /// Representa algo significativo que sucedió en el dominio.
    /// Los domain events son INTERNOS al bounded context (servicio).
    /// Para comunicarse con otros servicios, se mapean a Integration Events.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// ID único del evento. Útil para idempotencia, logging, debugging.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Cuándo ocurrió el evento (UTC).
        /// </summary>
        DateTime OccurredOnUtc { get; }
    }
}
