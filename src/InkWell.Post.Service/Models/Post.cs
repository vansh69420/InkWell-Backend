using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InkWell.Post.Service.Enums;

namespace InkWell.Post.Service.Models;

[Table("posts")]
public class BlogPost
{
    [Key]
    public Guid PostId { get; set; }

    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(350)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Excerpt { get; set; }

    [StringLength(500)]
    public string? FeaturedImageUrl { get; set; }

    [Required]
    public PostStatus Status { get; set; } = PostStatus.Draft;

    public int ReadTimeMin { get; set; } = 0;

    public int ViewCount { get; set; } = 0;

    public int LikesCount { get; set; } = 0;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

    public bool IsFeatured { get; set; } = false;
}