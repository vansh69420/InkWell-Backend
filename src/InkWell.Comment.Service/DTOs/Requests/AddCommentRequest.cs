namespace InkWell.Comment.Service.DTOs.Requests;

public class AddCommentRequest
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}