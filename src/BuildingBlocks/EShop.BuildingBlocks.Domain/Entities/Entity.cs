using EShop.BuildingBlocks.Domain.Abstractions;

namespace EShop.BuildingBlocks.Domain.Entities
{
    /// <summary>
    /// Clase base para todas las entidades del dominio.
    /// Implementa igualdad por IDENTIDAD (no por valores).
    /// 
    /// Ejemplo: dos productos con el mismo ID son la misma entidad,
    /// aunque tengan nombres diferentes (uno actualizado, otro stale).
    /// </summary>
    /// <typeparam name="TId">Tipo del ID. Típicamente Guid en sistemas distribuidos.</typeparam>
    public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : notnull
    {
        /// <summary>
        /// Constructor protegido que asegura que toda entidad nazca con un ID.
        /// </summary>
        protected Entity(TId id)
        {
            // Validación: NO podemos crear una entidad sin ID.
            // Esto previene bugs donde alguien crea una entidad y olvida asignarle ID.
            ArgumentNullException.ThrowIfNull(id);
            Id = id;
        }

        /// <summary>
        /// Constructor sin parámetros REQUERIDO por EF Core para hidratar desde la BD.
        /// EF necesita poder crear instancias vacías y luego llenar las propiedades.
        /// 
        /// Es 'protected' para que SOLO EF Core lo use (por reflection),
        /// nadie del dominio puede crear entidades sin ID.
        /// </summary>

#pragma warning disable CS8618 // Non-nullable field. EF Core lo poblará.
        protected Entity() { }
#pragma warning restore CS8618

        public TId Id { get; protected set; }

        /// <summary>
        /// Compara dos entidades por su ID, no por sus valores.
        /// Esta es LA regla central de DDD para entidades.
        /// </summary>
        public bool Equals(Entity<TId>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            // Importante: comparamos los TIPOS también, no solo los IDs.
            // Si tienes un Product con Id 5 y un Category con Id 5, NO son iguales.
            if (other.GetType() != GetType()) return false;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

        /// <summary>
        /// HashCode basado en el ID. Necesario para que las entidades funcionen
        /// correctamente en HashSet, Dictionary, etc.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(GetType(), Id);

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
            => Equals(left, right);

        public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
            => !Equals(left, right);
    }
}
