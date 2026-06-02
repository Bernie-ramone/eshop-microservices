using EShop.BuildingBlocks.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace EShop.BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que publica Domain Events vía MediatR DESPUÉS de un SaveChanges exitoso.
///
/// Flujo:
/// 1. SaveChanges() se ejecuta y commitea a BD.
/// 2. Este interceptor (SavedChangesAsync, NÓTESE EL TIEMPO PASADO) se dispara.
/// 3. Buscamos AggregateRoots con DomainEvents pendientes.
/// 4. Publicamos cada evento por MediatR.
/// 5. Limpiamos los eventos para que no se publiquen otra vez.
///
/// Los handlers de los Domain Events son los que típicamente generan
/// Integration Events y los persisten en el Outbox.
/// </summary>
public sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    // IPublisher es la interface de MediatR para publicar notifications.
    // Es más enfocada que IMediator (que también hace Send).
    private readonly IPublisher _publisher;

    public PublishDomainEventsInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Hook que se dispara DESPUÉS de un SaveChanges exitoso (versión async).
    /// </summary>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        }

        // Llamamos al base para mantener comportamiento default y retornar el result.
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Versión sincrónica. Block sobre la publicación async — no es ideal pero
    /// los SaveChanges sincrónicos son raros y normalmente solo en seed data.
    /// </summary>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null)
        {
            // GetAwaiter().GetResult() es la forma "menos peligrosa" de bloquear async.
            // .Wait() y .Result pueden causar deadlock; esto no.
            PublishDomainEventsAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavedChanges(eventData, result);
    }

    /// <summary>
    /// Lógica central: extrae eventos de los agregados y los publica.
    /// </summary>
    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Buscamos TODAS las entidades trackeadas que sean AggregateRoot
        // y que tengan eventos pendientes.
        // Usamos IAggregateRoot (no genérico) porque queremos polimorfismo
        // sin importar el tipo de Id del agregado.
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // ToList() es importante: materializa la query ANTES de iterar.
        // Si no, modificar los eventos durante el foreach podría afectar el ChangeTracker.

        // PASO 1: Recopilar TODOS los eventos antes de publicarlos.
        // Esto es importante porque si publicamos uno y modifica datos, no queremos
        // que aparezcan eventos nuevos en el medio del foreach.
        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // PASO 2: Limpiar los eventos de los agregados.
        // Lo hacemos ANTES de publicar para evitar re-publicación si por alguna
        // razón el flujo se ejecuta dos veces.
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // PASO 3: Publicar los eventos en orden de creación.
        // MediatR los entregará a todos los INotificationHandler<TEvent> registrados.
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
