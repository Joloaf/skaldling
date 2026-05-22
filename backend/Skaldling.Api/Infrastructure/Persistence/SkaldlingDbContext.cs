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
    public DbSet<Sprite> Sprites => Set<Sprite>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hero>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
            entity.Property(h => h.CreatedAt).IsRequired();
            entity.Property(h => h.UpdatedAt).IsRequired();
            if (Database.IsNpgsql())
            {
                entity.OwnsOne(h => h.AvatarConfig, owned => owned.ToJson());
            }
            else
            {
                entity.Ignore(h => h.AvatarConfig);
            }
        });

        modelBuilder.Entity<Sprite>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.Name).IsUnique();
            entity.Property(s => s.Type).HasConversion<string>().HasMaxLength(40);
            entity.Property(s => s.Description).IsRequired().HasMaxLength(500);
            entity.Property(s => s.AssetPath).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Layer).IsRequired();
            entity.Property(s => s.ArchetypeFamily).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.IsDefault).IsRequired();
            if (Database.IsNpgsql())
            {
                entity.Property(s => s.Tags).HasColumnType("jsonb");
            }
            else
            {
                entity.Ignore(s => s.Tags);
            }

            entity.HasData(SpriteSeedData.All);
        });
    }
}
