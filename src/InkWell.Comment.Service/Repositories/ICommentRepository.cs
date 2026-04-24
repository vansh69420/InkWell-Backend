using InkWell.Comment.Service.Models;

namespace InkWell.Comment.Service.Repositories;

public interface ICommentRepository
{
    Task<List<PostComment>> GetByPostIdAsync(Guid postId);
    Task<List<PostComment>> GetRepliesAsync(Guid parentCommentId);
    Task<PostComment?> GetByCommentIdAsync(Guid commentId);
    Task<int> CountByPostIdAsync(Guid postId);
    Task AddAsync(PostComment comment);
    Task SaveChangesAsync();
    Task<CommentLike?> GetLikeAsync(Guid commentId, Guid userId);
    Task AddLikeAsync(CommentLike like);
    Task RemoveLikeAsync(CommentLike like);
}