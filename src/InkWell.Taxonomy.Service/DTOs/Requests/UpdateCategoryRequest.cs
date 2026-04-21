namespace InkWell.Taxonomy.Service.DTOs.Requests;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    // Slug does NOT change unless you send it explicitly (as approved).
    public string? Slug { get; set; }

    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}