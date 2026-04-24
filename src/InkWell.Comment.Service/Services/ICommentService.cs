using InkWell.Comment.Service.DTOs.Requests;
using InkWell.Comment.Service.DTOs.Responses;

namespace InkWell.Comment.Service.Services;

public interface ICommentService
{
    Task<List<CommentResponse>> GetByPostAsync(Guid postId, Guid? currentUserId);
    Task<CommentResponse> AddCommentAsync(AddCommentRequest request, Guid authorId, string authorUsername, string authorFullName);
    Task<CommentResponse> UpdateCommentAsync(Guid commentId, UpdateCommentRequest request, Guid currentUserId);
    Task DeleteCommentAsync(Guid commentId, Guid currentUserId, bool isAdmin);
    Task<CommentResponse> LikeCommentAsync(Guid commentId, Guid userId);
    Task<CommentResponse> UnlikeCommentAsync(Guid commentId, Guid userId);
    Task<int> GetCommentCountAsync(Guid postId);
}