using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InkWell.Auth.Service.Enums;

namespace InkWell.Auth.Service.Models;

[Table("users")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Reader;

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [Required]
    public AuthProvider Provider { get; set; } = AuthProvider.Local;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
}