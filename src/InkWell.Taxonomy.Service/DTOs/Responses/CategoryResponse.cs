namespace InkWell.Taxonomy.Service.DTOs.Responses;

public class CategoryResponse
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
}