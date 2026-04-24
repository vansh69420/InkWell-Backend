using InkWell.Post.Service.DTOs.Responses;
using InkWell.Post.Service.DTOs.Requests;

namespace InkWell.Post.Service.Services
{
    public interface IPostService
    {
        Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync();
        Task<IReadOnlyList<PostSummaryResponse>> SearchPostsAsync(string keyword);
        Task<IReadOnlyList<PostSummaryResponse>> GetPostsByCategoryAsync(Guid categoryId);
        Task<IReadOnlyList<PostSummaryResponse>> GetPostsByTagAsync(Guid tagId);
        Task<AuthorPostsResponse> GetPostsByAuthorAsync(Guid authorId);
        Task<PostCountResponse> GetPostCountAsync();
        Task<bool> IncrementViewCountAsync(Guid postId);

        Task<IReadOnlyList<MyPostSummaryResponse>> GetMyPostsAsync(Guid currentUserId);

        Task<PostEditorResponse?> GetPostForEditAsync(Guid currentUserId, bool isAdmin, Guid postId);
        Task<PostEditorResponse> CreatePostAsync(Guid currentUserId, CreatePostRequest request);
        Task<PostEditorResponse?> UpdatePostAsync(Guid currentUserId, bool isAdmin, Guid postId, UpdatePostRequest request);

        Task<PostEditorResponse?> PublishPostAsync(Guid currentUserId, bool isAdmin, Guid postId);
        Task<PostEditorResponse?> UnpublishPostAsync(Guid currentUserId, bool isAdmin, Guid postId);

        Task<PostDetailResponse?> GetPostBySlugAsync(string slug, Guid? currentUserId = null);
        Task<bool> LikePostAsync(Guid postId, Guid userId);
        Task<bool> UnlikePostAsync(Guid postId, Guid userId);
        Task<bool> DeletePostAsync(Guid currentUserId, bool isAdmin, Guid postId);

    }
}