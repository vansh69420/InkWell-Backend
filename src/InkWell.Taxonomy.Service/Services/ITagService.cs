using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;

namespace InkWell.Taxonomy.Service.Services;

public interface ITagService
{
    Task<IReadOnlyList<TagResponse>> GetAllAsync();
    Task<TagResponse?> GetBySlugAsync(string slug);
    Task<TagResponse?> GetByIdAsync(Guid tagId);

    Task<TagResponse> CreateAsync(CreateTagRequest request);
    Task<TagResponse?> UpdateAsync(Guid tagId, UpdateTagRequest request);
    Task<bool> DeleteAsync(Guid tagId);
}