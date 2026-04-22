namespace InkWell.Post.Service.External;

public interface ITaxonomyClient
{
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default);
    Task<bool> TagExistsAsync(Guid tagId, CancellationToken ct = default);
}