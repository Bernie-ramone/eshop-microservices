namespace EShop.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Marca un evento como Integration Event (cruza servicios vía broker).
///
/// Diferencia clave con IDomainEvent:
/// - IDomainEvent: in-process, dentro del mismo bounded context.
/// - IIntegrationEvent: inter-proceso, vía RabbitMQ/MassTransit.
///
/// Los Integration Events son contractuales: otros servicios DEPENDEN de
/// su forma. Cambiarlos rompe consumidores. Por eso suelen versionarse
/// (OrderPlacedV1, OrderPlacedV2) o tener forwards compatibility.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// ID único del evento. Crítico para idempotencia.
    /// El consumidor verifica si ya procesó este ID antes de actuar.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Timestamp UTC de cuándo se generó el evento.
    /// Útil para ordering best-effort y logging.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}