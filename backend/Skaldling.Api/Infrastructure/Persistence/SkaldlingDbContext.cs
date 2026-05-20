using Microsoft.EntityFrameworkCore;
using Skaldling.Api.Domain;

namespace Skaldling.Api.Infrastructure.Persistence;

public class SkaldlingDbContext : DbContext
{
    public SkaldlingDbContext(DbContextOptions<SkaldlingDbContext> options)
        : base(options)
    {
    }
    public DbSet<Hero> Heroes => Set<Hero>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hero>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
            entity.Property(h => h.CreatedAt).IsRequired();
            entity.Property(h => h.UpdatedAt).IsRequired();
        });
    }
}