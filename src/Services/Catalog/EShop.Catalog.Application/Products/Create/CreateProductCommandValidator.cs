using FluentValidation;

namespace EShop.Catalog.Application.Products.Create;

/// <summary>
/// Validador de CreateProductCommand.
///
/// IMPORTANTE: esta validación es de FORMATO/SINTAXIS (input shape),
/// NO de reglas de negocio. Las reglas de negocio (SKU único, categoría existe)
/// se validan DENTRO del handler, porque requieren consultar la BD.
///
/// FluentValidation se ejecuta automáticamente por el ValidationBehavior
/// (Application BuildingBlock) ANTES de que el handler se ejecute.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
            .When(x => x.Description is not null); // Solo valida si no es null

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO 4217 code.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");
    }
}