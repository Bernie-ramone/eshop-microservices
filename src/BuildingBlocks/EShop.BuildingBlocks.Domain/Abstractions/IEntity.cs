namespace EShop.BuildingBlocks.Domain.Abstractions
{
    /// <summary>
    /// Marker interface para identificar entidades del dominio.
    /// Una entidad tiene IDENTIDAD propia (definida por su ID),
    /// y dos entidades son iguales si tienen el mismo ID.
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// Entity tipada con un ID específico.
    /// El generic permite usar Guid, int, long, o un strongly-typed ID.
    /// </summary>
    public interface IEntity<TId> : IEntity
    {
        TId Id { get; }
    }
}
