using EShop.Catalog.Application.Abstractions;
using EShop.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;


namespace EShop.Catalog.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(CategoryId id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(Category category)
    {
        _context.Categories.Add(category);
    }
}