namespace EShop.BuildingBlocks.Domain.Abstractions
{
    /// <summary>
    /// Marker interface que identifica al root de un agregado.
    /// Solo los aggregate roots pueden ser cargados directamente del repositorio.
    /// Las entidades hijas SOLO se acceden a través del root.
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// Eventos de dominio que el agregado ha generado pero aún no se han publicado.
        /// El Infrastructure layer los lee y los procesa después de un SaveChanges exitoso.
        /// </summary>
        IReadOnlyList<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Limpia los eventos después de publicarlos para evitar duplicados.
        /// </summary>
        void ClearDomainEvents();
    }


    /// <summary>
    /// AggregateRoot tipado con un ID específico.
    /// </summary>
    public interface IAggregateRoot<TId> : IAggregateRoot, IEntity<TId>
    {
    }
}
