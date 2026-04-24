using InkWell.Communication.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Communication.Service.DbContexts;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options)
        : base(options) { }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscriber>(e =>
        {
            e.HasKey(s => s.SubscriberId);
            e.HasIndex(s => s.Email).IsUnique();
            e.HasIndex(s => s.Token).IsUnique();
            e.Property(s => s.Email).IsRequired();
            e.Property(s => s.Status).IsRequired();
            e.Property(s => s.Token).IsRequired();
        });
    }
}