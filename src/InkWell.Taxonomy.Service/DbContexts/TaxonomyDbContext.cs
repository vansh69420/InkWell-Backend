using InkWell.Taxonomy.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Taxonomy.Service.DbContexts;

public class TaxonomyDbContext : DbContext
{
    public TaxonomyDbContext(DbContextOptions<TaxonomyDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);

            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Slug).IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();

            entity.Property(x => x.PostCount).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(x => x.TagId);

            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Slug).IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();

            entity.Property(x => x.PostCount).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }
}