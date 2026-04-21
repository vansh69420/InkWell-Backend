using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkWell.Post.Service.Models;

[Table("post_likes")]
public class PostLike
{
    [Key]
    public Guid PostLikeId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BlogPost BlogPost { get; set; } = null!;
}