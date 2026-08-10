using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.Products;

namespace EShop.Catalog.Application.Abstractions;

/// <summary>
/// Abstracción de SOLO LECTURA para queries que necesitan proyecciones
/// eficientes (Select directo a DTO, sin cargar agregados completos).
///
/// Por qué esto NO rompe Clean Architecture:
/// - Application define QUÉ necesita leer (IQueryable de sus propias entidades).
/// - Infrastructure implementa esto exponiendo los DbSet de su DbContext.
/// - Application sigue sin saber que existe EF Core o SQL Server.
///
/// Es DELIBERADAMENTE distinto de IProductRepository: ese es para escritura
/// (carga agregados completos, trackea cambios). Este es para lectura
/// (proyecciones, sin tracking, alta performance).
/// </summary>
public interface ICatalogReadContext
{
    IQueryable<Product> Products { get; }
    IQueryable<Category> Categories { get; }
}