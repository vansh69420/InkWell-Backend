using InkWell.Comment.Service.DTOs.Requests;
using InkWell.Comment.Service.DTOs.Responses;
using InkWell.Comment.Service.Enums;
using InkWell.Comment.Service.Models;
using InkWell.Comment.Service.Repositories;

namespace InkWell.Comment.Service.Services;

public class CommentServiceImpl : ICommentService
{
    private readonly ICommentRepository _repo;

    public CommentServiceImpl(ICommentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CommentResponse>> GetByPostAsync(Guid postId, Guid? currentUserId)
    {
        var comments = await _repo.GetByPostIdAsync(postId);
        var result = new List<CommentResponse>();

        foreach (var comment in comments)
        {
            var replies = await _repo.GetRepliesAsync(comment.CommentId);
            var mapped = Map(comment, currentUserId);
            mapped.Replies = replies.Select(r => Map(r, currentUserId)).ToList();
            result.Add(mapped);
        }

        return result;
    }

    public async Task<CommentResponse> AddCommentAsync(
        AddCommentRequest request,
        Guid authorId,
        string authorUsername,
        string authorFullName)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content cannot be empty.");

        if (request.Content.Length > 2000)
            throw new ArgumentException("Comment cannot exceed 2000 characters.");

        var comment = new PostComment
        {
            PostId = request.PostId,
            AuthorId = authorId,
            AuthorUsername = authorUsername,
            AuthorFullName = authorFullName,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim(),
            Status = CommentStatus.Approved
        };

        await _repo.AddAsync(comment);
        await _repo.SaveChangesAsync();

        return Map(comment, authorId);
    }

    public async Task<CommentResponse> UpdateCommentAsync(
        Guid commentId,
        UpdateCommentRequest request,
        Guid currentUserId)
    {
        var comment = await _repo.GetByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != currentUserId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        if (comment.Status == CommentStatus.Deleted)
            throw new InvalidOperationException("Cannot edit a deleted comment.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content cannot be empty.");

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return Map(comment, currentUserId);
    }

    public async Task DeleteCommentAsync(
        Guid commentId,
        Guid currentUserId,
        bool isAdmin)
    {
        var comment = await _repo.GetByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (!isAdmin && comment.AuthorId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        comment.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();
    }

    public async Task<CommentResponse> LikeCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _repo.GetByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        var existing = await _repo.GetLikeAsync(commentId, userId);
        if (existing != null)
            throw new InvalidOperationException("Already liked.");

        var like = new CommentLike
        {
            CommentId = commentId,
            UserId = userId
        };

        await _repo.AddLikeAsync(like);
        comment.LikesCount++;
        await _repo.SaveChangesAsync();

        return Map(comment, userId);
    }

    public async Task<CommentResponse> UnlikeCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _repo.GetByCommentIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        var like = await _repo.GetLikeAsync(commentId, userId);
        if (like == null)
            throw new InvalidOperationException("Not liked yet.");

        await _repo.RemoveLikeAsync(like);
        comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
        await _repo.SaveChangesAsync();

        return Map(comment, userId);
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _repo.CountByPostIdAsync(postId);
    }

    private CommentResponse Map(PostComment c, Guid? currentUserId)
    {
        return new CommentResponse
        {
            CommentId = c.CommentId,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            AuthorUsername = c.AuthorUsername,
            AuthorFullName = c.AuthorFullName,
            ParentCommentId = c.ParentCommentId,
            Content = c.Status == CommentStatus.Deleted
                ? "[deleted]"
                : c.Content,
            LikesCount = c.LikesCount,
            IsLikedByCurrentUser = currentUserId.HasValue &&
                                   c.Likes.Any(l => l.UserId == currentUserId.Value),
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
    public async Task<int> GetTotalCommentCountAsync()
    {
        return await _repo.GetTotalCountAsync();
    }
}