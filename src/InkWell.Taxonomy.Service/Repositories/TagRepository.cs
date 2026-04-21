using InkWell.Taxonomy.Service.DbContexts;
using InkWell.Taxonomy.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Taxonomy.Service.Repositories;

public class TagRepository : ITagRepository
{
    private readonly TaxonomyDbContext _context;

    public TagRepository(TaxonomyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(Guid tagId)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(x => x.TagId == tagId);
    }

    public async Task<Tag?> GetBySlugAsync(string slug)
    {
        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? ignoreTagId = null)
    {
        return await _context.Tags
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug && (ignoreTagId == null || x.TagId != ignoreTagId));
    }

    public async Task AddAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task DeleteAsync(Tag tag)
    {
        _context.Tags.Remove(tag);
        return Task.CompletedTask;
    }
}