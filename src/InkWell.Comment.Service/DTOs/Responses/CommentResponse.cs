namespace InkWell.Comment.Service.DTOs.Responses;

public class CommentResponse
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();
}