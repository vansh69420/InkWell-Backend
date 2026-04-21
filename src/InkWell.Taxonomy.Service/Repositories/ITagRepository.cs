using InkWell.Taxonomy.Service.Models;

namespace InkWell.Taxonomy.Service.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(Guid tagId);
    Task<Tag?> GetBySlugAsync(string slug);

    Task<bool> SlugExistsAsync(string slug, Guid? ignoreTagId = null);

    Task AddAsync(Tag tag);
    Task SaveChangesAsync();
    Task DeleteAsync(Tag tag);
}