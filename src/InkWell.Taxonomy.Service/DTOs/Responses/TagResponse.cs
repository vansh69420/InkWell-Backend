namespace InkWell.Taxonomy.Service.DTOs.Responses;

public class TagResponse
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
}