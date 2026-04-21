using InkWell.Taxonomy.Service.Models;

namespace InkWell.Taxonomy.Service.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid categoryId);
    Task<Category?> GetBySlugAsync(string slug);

    Task<bool> SlugExistsAsync(string slug, Guid? ignoreCategoryId = null);
    Task<bool> HasChildrenAsync(Guid categoryId);

    Task AddAsync(Category category);
    Task SaveChangesAsync();
    Task DeleteAsync(Category category);
}