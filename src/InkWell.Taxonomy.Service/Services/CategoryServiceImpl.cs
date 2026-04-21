using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;
using InkWell.Taxonomy.Service.Models;
using InkWell.Taxonomy.Service.Repositories;
using InkWell.Taxonomy.Service.Utilities;

namespace InkWell.Taxonomy.Service.Services;

public class CategoryServiceImpl : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryServiceImpl(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();
        return categories.Select(Map).ToList();
    }

    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var category = await _repo.GetBySlugAsync(slug);
        return category == null ? null : Map(category);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid categoryId)
    {
        var category = await _repo.GetByIdAsync(categoryId);
        return category == null ? null : Map(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : SlugGenerator.Generate(request.Slug);

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug could not be generated.");

        if (await _repo.SlugExistsAsync(slug))
            throw new InvalidOperationException("Slug already exists.");

        var entity = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            PostCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid categoryId, UpdateCategoryRequest request)
    {
        var entity = await _repo.GetByIdAsync(categoryId);
        if (entity == null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.ParentCategoryId = request.ParentCategoryId;

        // Slug stays the same unless explicitly provided
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var newSlug = SlugGenerator.Generate(request.Slug);
            if (string.IsNullOrWhiteSpace(newSlug))
                throw new ArgumentException("Slug is invalid.");

            if (await _repo.SlugExistsAsync(newSlug, ignoreCategoryId: categoryId))
                throw new InvalidOperationException("Slug already exists.");

            entity.Slug = newSlug;
        }

        await _repo.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid categoryId)
    {
        var entity = await _repo.GetByIdAsync(categoryId);
        if (entity == null) return false;

        // Block delete if it has children
        if (await _repo.HasChildrenAsync(categoryId))
            throw new InvalidOperationException("Cannot delete category that has child categories.");

        await _repo.DeleteAsync(entity);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static CategoryResponse Map(Category c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        PostCount = c.PostCount,
        CreatedAt = c.CreatedAt
    };
}