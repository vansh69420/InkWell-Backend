using InkWell.Media.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Media.Service.DbContexts;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

    public DbSet<MediaFile> Media => Set<MediaFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaFile>(e =>
        {
            e.HasKey(m => m.MediaId);
            e.Property(m => m.Filename).IsRequired();
            e.Property(m => m.OriginalName).IsRequired();
            e.Property(m => m.Url).IsRequired();
            e.Property(m => m.MimeType).IsRequired();
            e.HasIndex(m => m.UploaderId);
            e.HasIndex(m => m.LinkedPostId);
        });
    }
}