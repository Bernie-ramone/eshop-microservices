namespace EShop.BuildingBlocks.Domain.ValueObjects
{
    /// <summary>
    /// Clase base para Value Objects.
    /// 
    /// La igualdad se calcula comparando TODOS los componentes que define la subclase.
    /// Las subclases deben implementar GetEqualityComponents() para indicar qué se compara.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Las subclases devuelven los componentes que definen igualdad.
        /// Ejemplo: para Money, serían [Amount, Currency].
        /// </summary>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public bool Equals(ValueObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            // SequenceEqual compara las dos listas elemento por elemento.
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override bool Equals(object? obj) => Equals(obj as ValueObject);

        public override int GetHashCode()
        {
            // Combinamos los hash codes de todos los componentes.
            // Aggregate va aplicando una función a cada elemento, partiendo de un seed.
            return GetEqualityComponents()
                .Aggregate(0, (hash, component) =>
                    HashCode.Combine(hash, component?.GetHashCode() ?? 0));
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
            => Equals(left, right);

        public static bool operator !=(ValueObject? left, ValueObject? right)
            => !Equals(left, right);
    }
}
