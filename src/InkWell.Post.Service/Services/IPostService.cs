using InkWell.Post.Service.DTOs.Responses;

namespace InkWell.Post.Service.Services
{
    public interface IPostService
    {
        Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync();
        Task<PostDetailResponse?> GetPostBySlugAsync(string slug);
        Task<IReadOnlyList<PostSummaryResponse>> SearchPostsAsync(string keyword);
        Task<IReadOnlyList<PostSummaryResponse>> GetPostsByCategoryAsync(Guid categoryId);
        Task<IReadOnlyList<PostSummaryResponse>> GetPostsByTagAsync(Guid tagId);
        Task<AuthorPostsResponse> GetPostsByAuthorAsync(Guid authorId);
        Task<PostCountResponse> GetPostCountAsync();
        Task<bool> IncrementViewCountAsync(Guid postId);
    }
}