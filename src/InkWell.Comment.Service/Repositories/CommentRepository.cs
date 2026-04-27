using InkWell.Comment.Service.DbContexts;
using InkWell.Comment.Service.Enums;
using InkWell.Comment.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Comment.Service.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _db;

    public CommentRepository(CommentDbContext db)
    {
        _db = db;
    }

    public async Task<List<PostComment>> GetByPostIdAsync(Guid postId)
    {
        return await _db.Comments
            .Include(c => c.Likes)
            .Where(c => c.PostId == postId
                     && c.ParentCommentId == null
                     && c.Status != CommentStatus.Deleted
                     && c.Status != CommentStatus.Rejected)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PostComment>> GetRepliesAsync(Guid parentCommentId)
    {
        return await _db.Comments
            .Include(c => c.Likes)
            .Where(c => c.ParentCommentId == parentCommentId
                     && c.Status != CommentStatus.Deleted
                     && c.Status != CommentStatus.Rejected)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<PostComment?> GetByCommentIdAsync(Guid commentId)
    {
        return await _db.Comments
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }

    public async Task<int> CountByPostIdAsync(Guid postId)
    {
        return await _db.Comments
            .CountAsync(c => c.PostId == postId
                          && c.Status == CommentStatus.Approved);
    }

    public async Task AddAsync(PostComment comment)
    {
        await _db.Comments.AddAsync(comment);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task<CommentLike?> GetLikeAsync(Guid commentId, Guid userId)
    {
        return await _db.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId
                                   && l.UserId == userId);
    }

    public async Task AddLikeAsync(CommentLike like)
    {
        await _db.CommentLikes.AddAsync(like);
    }

    public Task RemoveLikeAsync(CommentLike like)
    {
        _db.CommentLikes.Remove(like);
        return Task.CompletedTask;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _db.Set<PostComment>()
            .CountAsync(c => c.Status != CommentStatus.Deleted);
    }
}