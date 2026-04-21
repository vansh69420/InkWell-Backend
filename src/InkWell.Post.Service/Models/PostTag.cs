using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkWell.Post.Service.Models;

[Table("post_tags")]
public class PostTag
{
    [Key]
    public Guid PostTagId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid TagId { get; set; }

    public BlogPost BlogPost { get; set; } = null!;
}