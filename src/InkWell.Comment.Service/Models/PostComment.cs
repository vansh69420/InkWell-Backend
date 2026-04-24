using InkWell.Comment.Service.Enums;

namespace InkWell.Comment.Service.Models;

public class PostComment
{
    public Guid CommentId { get; set; } = Guid.NewGuid();
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikesCount { get; set; } = 0;
    public CommentStatus Status { get; set; } = CommentStatus.Approved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}