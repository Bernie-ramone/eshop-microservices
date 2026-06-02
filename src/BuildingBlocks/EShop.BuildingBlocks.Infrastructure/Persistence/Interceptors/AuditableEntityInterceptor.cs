using EShop.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor que pobla automáticamente CreatedAtUtc/UpdatedAtUtc
/// en entidades que implementan IAuditableEntity.
///
/// Se ejecuta JUSTO ANTES de SaveChanges, así nos aseguramos de que los
/// timestamps reflejen el momento real del guardado.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Override del hook que se ejecuta antes del SaveChanges async.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // eventData.Context puede ser null en escenarios raros (ej: tests).
        // Si es null, no hacemos nada y dejamos pasar.
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        // Llamamos al base para mantener el comportamiento default.
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Versión sincrónica del interceptor.
    /// EF Core puede llamar a SaveChanges() o SaveChangesAsync() — overrideamos ambos
    /// para que la auditoría funcione en cualquier caso.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Recorre las entidades trackeadas, identifica las auditables,
    /// y pobla los timestamps según su estado.
    /// </summary>
    private static void UpdateAuditableEntities(DbContext context)
    {
        // ChangeTracker.Entries<T>() filtra solo entidades del tipo T.
        // Esto es MÁS EFICIENTE que iterar todas y hacer "is IAuditableEntity".
        foreach (EntityEntry<IAuditableEntity> entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            // DateTime.UtcNow es nuestro source of truth para timestamps.
            // NUNCA uses DateTime.Now en backend — depende del huso horario del servidor.
            var utcNow = DateTime.UtcNow;

            // Si la entidad es NUEVA (Added), seteamos CreatedAt.
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
            }

            // Si la entidad fue MODIFICADA (incluyendo Added recién agregada),
            // seteamos UpdatedAt.
            // ¿Por qué incluimos Added? Porque algunas convenciones quieren
            // que CreatedAt = UpdatedAt en creación. Si NO quieres ese
            // comportamiento, cambia el if a "State == Modified" solamente.
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
            }
        }
    }
}
