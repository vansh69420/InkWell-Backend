namespace InkWell.Taxonomy.Service.DTOs.Requests;

public class UpdateTagRequest
{
    public string Name { get; set; } = string.Empty;

    // Slug does NOT change unless you send it explicitly (as approved).
    public string? Slug { get; set; }
}