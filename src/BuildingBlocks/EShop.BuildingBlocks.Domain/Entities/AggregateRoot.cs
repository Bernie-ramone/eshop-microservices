using EShop.BuildingBlocks.Domain.Abstractions;

namespace EShop.BuildingBlocks.Domain.Entities
{
    /// <summary>
    /// Clase base para todos los aggregate roots.
    /// Hereda de Entity y agrega la capacidad de generar Domain Events.
    /// </summary>
    public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
        where TId : notnull
    {
        // Lista privada para que NADIE de fuera pueda agregar/quitar eventos directamente.
        // Solo el agregado mismo controla qué eventos dispara.
        private readonly List<IDomainEvent> _domainEvents = [];

        protected AggregateRoot(TId id) : base(id) { }

        protected AggregateRoot() { }

        /// <summary>
        /// Vista de SOLO LECTURA de los eventos pendientes.
        /// El Infrastructure layer leerá esta lista para publicar los eventos.
        /// </summary>
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Método PROTECTED para que SOLO el agregado mismo (o sus subclases) pueda agregar eventos.
        /// Esto encapsula la generación de eventos dentro de la lógica de negocio.
        /// 
        /// Ejemplo: dentro del método Order.Confirm(), llamas a RaiseDomainEvent(new OrderConfirmed(...))
        /// </summary>
        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Limpia los eventos. Lo invoca el Infrastructure layer DESPUÉS de publicarlos
        /// para evitar duplicados en el siguiente SaveChanges.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
