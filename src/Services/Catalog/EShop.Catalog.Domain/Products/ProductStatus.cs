namespace EShop.Catalog.Domain.Products;

/// <summary>
/// Estados posibles de un Product en el catálogo.
///
/// Usamos un enum (no clases con polymorphism) porque:
/// 1. Solo tenemos 2 estados ahora, no anticipamos comportamiento polimórfico.
/// 2. Los estados NO tienen comportamiento propio - solo afectan reglas de Product.
/// 3. Persistencia simple (un int en BD).
///
/// Si en el futuro cada estado tuviera lógica propia (ej: comportamiento de
/// publicación, cálculo de precios diferenciado), migraríamos a State Pattern.
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Activo y disponible para compra.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Descontinuado - no se vende más pero queda en histórico.
    /// NO se borra: las órdenes pasadas siguen referenciándolo.
    /// </summary>
    Discontinued = 2
}