using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkWell.Auth.Service.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    public Guid RefreshTokenId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    [Required]
    public bool IsRevoked { get; set; } = false;

    [StringLength(500)]
    public string? ReplacedByTokenHash { get; set; }

    public User? User { get; set; }
}