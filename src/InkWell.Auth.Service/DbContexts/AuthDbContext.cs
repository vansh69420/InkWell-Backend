using InkWell.Auth.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.DbContexts;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.UserId);

            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.Username).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Bio).HasMaxLength(500);
            entity.Property(x => x.AvatarUrl).HasMaxLength(500);
            entity.Property(x => x.Provider).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasMany(x => x.ExternalLogins)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.RefreshTokenId);

            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);

            entity.Property(x => x.TokenHash).IsRequired().HasMaxLength(500);
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.IsRevoked).HasDefaultValue(false);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalLogin>(entity =>
        {
            entity.ToTable("external_logins");
            entity.HasKey(x => x.ExternalLoginId);

            entity.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
            entity.HasIndex(x => x.UserId);

            entity.Property(x => x.ProviderUserId).IsRequired().HasMaxLength(255);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.LastLoginAt).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.ExternalLogins)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.AuditLogId);
            e.HasIndex(a => a.ActorId);
            e.HasIndex(a => a.CreatedAt);
            e.Property(a => a.Action).IsRequired();
            e.Property(a => a.EntityType).IsRequired();
        });
    }
}