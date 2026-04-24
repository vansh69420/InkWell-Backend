using InkWell.Comment.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Comment.Service.DbContexts;

public class CommentDbContext : DbContext
{
    public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options) { }

    public DbSet<PostComment> Comments => Set<PostComment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostComment>(e =>
        {
            e.HasKey(c => c.CommentId);
            e.Property(c => c.Content).IsRequired();
            e.Property(c => c.AuthorUsername).IsRequired();
            e.HasMany(c => c.Likes)
             .WithOne(l => l.Comment)
             .HasForeignKey(l => l.CommentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentLike>(e =>
        {
            e.HasKey(l => l.CommentLikeId);
            e.HasIndex(l => new { l.CommentId, l.UserId }).IsUnique();
        });
    }
}