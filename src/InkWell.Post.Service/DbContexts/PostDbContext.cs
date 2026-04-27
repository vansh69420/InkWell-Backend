using InkWell.Post.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Post.Service.DbContexts;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
    {
    }

    public DbSet<BlogPost> Posts => Set<BlogPost>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(x => x.PostId);

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.AuthorId);
            entity.HasIndex(x => x.Status);

            entity.Property(x => x.Title).IsRequired().HasMaxLength(300);
            entity.Property(x => x.Slug).IsRequired().HasMaxLength(350);
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.Excerpt).HasMaxLength(500);
            entity.Property(x => x.FeaturedImageUrl).HasMaxLength(500);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.ReadTimeMin).HasDefaultValue(0);
            entity.Property(x => x.ViewCount).HasDefaultValue(0);
            entity.Property(x => x.LikesCount).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.IsFeatured).HasDefaultValue(false);

            entity.HasMany(x => x.PostCategories)
                .WithOne(x => x.BlogPost)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.PostTags)
                .WithOne(x => x.BlogPost)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.ToTable("post_categories");
            entity.HasKey(x => x.PostCategoryId);
            entity.HasIndex(x => new { x.PostId, x.CategoryId }).IsUnique();
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.ToTable("post_tags");
            entity.HasKey(x => x.PostTagId);
            entity.HasIndex(x => new { x.PostId, x.TagId }).IsUnique();
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.ToTable("post_likes");
            entity.HasKey(x => x.PostLikeId);
            entity.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();

            entity.HasOne(x => x.BlogPost)
                .WithMany()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}