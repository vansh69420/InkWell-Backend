namespace InkWell.Media.Service.DTOs.Responses;

public class MediaResponse
{
    public Guid MediaId { get; set; }
    public Guid UploaderId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeKb { get; set; }
    public string? AltText { get; set; }
    public Guid? LinkedPostId { get; set; }
    public DateTime UploadedAt { get; set; }
}