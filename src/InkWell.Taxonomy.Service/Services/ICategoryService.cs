using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;

namespace InkWell.Taxonomy.Service.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    Task<CategoryResponse?> GetByIdAsync(Guid categoryId);

    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(Guid categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(Guid categoryId);
}