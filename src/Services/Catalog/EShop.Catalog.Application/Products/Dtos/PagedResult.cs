namespace EShop.Catalog.Application.Products.Dtos;

/// <summary>
/// Envoltorio genérico para resultados paginados.
/// Reutilizable para CUALQUIER entidad paginada (Products, Categories, Orders, etc).
///
/// Incluye metadata de paginación para que el cliente sepa:
/// - En qué página está
/// - Cuántas páginas hay en total
/// - Si hay página siguiente/anterior
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Calculado: total de páginas redondeando hacia arriba.
    /// Ejemplo: 105 items / 20 por página = 5.25 → 6 páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}