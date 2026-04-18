using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InkWell.Auth.Service.Enums;

namespace InkWell.Auth.Service.Models;

[Table("external_logins")]
public class ExternalLogin
{
    [Key]
    public Guid ExternalLoginId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public AuthProvider Provider { get; set; }

    [Required]
    [StringLength(255)]
    public string ProviderUserId { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Email { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}