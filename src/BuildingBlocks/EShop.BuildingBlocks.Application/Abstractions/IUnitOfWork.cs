namespace EShop.BuildingBlocks.Application.Abstractions;


/// <summary>
/// Abstracción de Unit of Work: agrupa cambios y los persiste atómicamente.
///
/// La implementación concreta es el DbContext de cada servicio
/// (CatalogDbContext, OrderingDbContext, etc), que YA implementa
/// SaveChangesAsync nativamente - solo "adoptamos" la interface.
///
/// Application depende de ESTA abstracción, nunca de DbContext directamente.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todos los cambios trackeados en la transacción actual.
    /// Aquí es donde los interceptors (Auditable, PublishDomainEvents) se activan.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}