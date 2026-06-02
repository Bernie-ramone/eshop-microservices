namespace EShop.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Abstracción del bus de mensajes para publicar Integration Events.
///
/// La implementación concreta (MassTransit + RabbitMQ) vive en cada servicio.
/// Los handlers y el Outbox processor dependen de ESTA interface, no de
/// MassTransit directamente — esto es Dependency Inversion Principle.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica un evento al broker.
    /// "Publish" en messaging significa fan-out: el broker decide quién lo recibe
    /// según las suscripciones (a diferencia de "Send" que va a una cola específica).
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}
