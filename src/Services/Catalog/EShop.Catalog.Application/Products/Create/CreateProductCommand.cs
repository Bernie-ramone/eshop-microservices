using EShop.BuildingBlocks.Application.Abstractions.Messaging;

namespace EShop.Catalog.Application.Products.Create;

/// <summary>
/// Comando para crear un nuevo Product.
///
/// Es un 'record' porque:
/// - Inmutable: una vez creado, no cambia.
/// - Value equality: útil en tests ("assert que se llamó con este comando exacto").
///
/// Devuelve Guid (el Id del producto creado) para que el cliente pueda
/// hacer un GET inmediato o construir el header Location del 201 Created.
/// </summary>
public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId) : ICommand<Guid>;
