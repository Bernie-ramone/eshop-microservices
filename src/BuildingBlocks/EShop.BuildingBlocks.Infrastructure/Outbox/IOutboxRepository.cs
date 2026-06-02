using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Abstracción del repositorio de mensajes outbox.
///
/// La implementación concreta (con EF Core) vivirá en cada servicio.
/// Esta abstracción permite testear el OutboxProcessor sin BD real.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Agrega un mensaje al outbox. El commit a BD lo hace el SaveChanges del DbContext,
    /// NO este método. Por eso este método es síncrono — solo agrega al ChangeTracker.
    /// </summary>
    void Add(OutboxMessage message);

    /// <summary>
    /// Obtiene los mensajes pendientes (no procesados).
    /// Limitamos con batchSize para que el processor no se atragante con miles a la vez.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda los cambios (marcado como procesados, errores, etc).
    /// Wrapper del SaveChangesAsync del DbContext.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}