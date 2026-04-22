namespace InkWell.Post.Service.DTOs.Requests;

public class UpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }

    public List<Guid> CategoryIds { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}