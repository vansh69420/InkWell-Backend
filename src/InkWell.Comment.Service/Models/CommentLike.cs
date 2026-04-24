namespace InkWell.Comment.Service.Models;

public class CommentLike
{
    public Guid CommentLikeId { get; set; } = Guid.NewGuid();
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    public PostComment Comment { get; set; } = null!;
}