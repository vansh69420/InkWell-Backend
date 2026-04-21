using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkWell.Post.Service.Models;

[Table("post_categories")]
public class PostCategory
{
    [Key]
    public Guid PostCategoryId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public BlogPost BlogPost { get; set; } = null!;
}