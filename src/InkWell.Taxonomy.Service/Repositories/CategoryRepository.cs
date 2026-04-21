using InkWell.Taxonomy.Service.DbContexts;
using InkWell.Taxonomy.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Taxonomy.Service.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly TaxonomyDbContext _context;

    public CategoryRepository(TaxonomyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid categoryId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x => x.CategoryId == categoryId);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? ignoreCategoryId = null)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug && (ignoreCategoryId == null || x.CategoryId != ignoreCategoryId));
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.ParentCategoryId == categoryId);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        return Task.CompletedTask;
    }
}