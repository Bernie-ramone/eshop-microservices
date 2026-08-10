using EShop.Catalog.Domain.Categories;
using EShop.Catalog.Domain.Products;
using EShop.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para Product.
///
/// Usamos IEntityTypeConfiguration<T> (Fluent API) en vez de Data Annotations
/// porque:
/// 1. Mantiene el dominio LIMPIO de atributos de infraestructura (Product.cs
///    no tiene ni un solo [Column] o [Required]).
/// 2. Más expresivo para configuraciones complejas (Owned Entities, converters).
/// 3. Testeable independientemente.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // ============================================================
        // VALUE CONVERTER para ProductId (strongly-typed ID)
        // ============================================================
        // Sin esto, EF Core no sabe cómo persistir un ProductId (record struct).
        // Le enseñamos: al guardar, extrae el Guid interno (.Value).
        // Al leer, envuelve el Guid en un nuevo ProductId.
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value))
            .ValueGeneratedNever(); // El Id se genera en el dominio (ProductId.New()), no en la BD.

        // ============================================================
        // SKU: Value Object simple, un solo valor -> usamos conversion directa
        // ============================================================
        builder.Property(p => p.Sku)
            .HasConversion(
                sku => sku.Value,
                value => Sku.Create(value))
            .HasColumnName("Sku")
            .HasMaxLength(50)
            .IsRequired();

        // Índice único: NUNCA puede haber dos productos con el mismo SKU.
        // Esto es la garantía a nivel BD; ya validamos en el Handler también
        // (defensa en profundidad), pero la BD es la última línea de defensa
        // contra race conditions (dos requests simultáneos con el mismo SKU).
        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        // ============================================================
        // MONEY como OWNED ENTITY
        // ============================================================
        // OwnsOne mapea el Value Object a columnas EN LA MISMA TABLA Products,
        // con el prefijo que definamos. No es una tabla separada.
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("Price_Amount")
                .HasColumnType("decimal(18,2)") // Precisión explícita para dinero
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("Price_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Navigation property requerida - un Product SIEMPRE tiene Price.
        // (Money no es nullable en el dominio, así que esto es consistente.)
        builder.Navigation(p => p.Price).IsRequired();

        // ============================================================
        // CategoryId: Value Converter (mismo patrón que ProductId)
        // ============================================================
        builder.Property(p => p.CategoryId)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value))
            .IsRequired();

        // Foreign Key explícita hacia Categories.
        // NO configuramos una navigation property Product.Category porque
        // el dominio NO la tiene (Product solo guarda CategoryId, no el objeto
        // Category completo - así evitamos acoplar los agregados).
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir borrar categoría con productos.

        // ============================================================
        // Status: enum guardado como string (más legible en BD que un int)
        // ============================================================
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ============================================================
        // Ignoramos DomainEvents - NO es una propiedad persistible.
        // Es una lista en memoria que el interceptor lee y limpia.
        // ============================================================
        builder.Ignore(p => p.DomainEvents);
    }
}