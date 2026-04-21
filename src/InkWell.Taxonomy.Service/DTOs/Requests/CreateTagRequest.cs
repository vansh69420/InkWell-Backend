namespace InkWell.Taxonomy.Service.DTOs.Requests;

public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}