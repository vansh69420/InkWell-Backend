namespace InkWell.Taxonomy.Service.DTOs.Requests;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}