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
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<Adventure> Adventures => Set<Adventure>();
    public DbSet<Day> Days => Set<Day>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<AdventureTask> AdventureTasks => Set<AdventureTask>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hero>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
            entity.Property(h => h.ReadingAge).IsRequired();
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

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
            entity.Property(t => t.Description).IsRequired().HasMaxLength(500);
            entity.Property(t => t.IsDefault).IsRequired();
            entity.HasData(ThemeSeedData.All);
        });

        modelBuilder.Entity<Adventure>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Tone).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.NarrativeStyle).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Moral).HasMaxLength(500);
            entity.Property(a => a.FinaleReward).HasMaxLength(200);
            entity.Property(a => a.CreatedAt).IsRequired();
            entity.Property(a => a.UpdatedAt).IsRequired();

            entity.HasOne(a => a.Hero)
                .WithMany()
                .HasForeignKey(a => a.HeroId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Theme)
                .WithMany()
                .HasForeignKey(a => a.ThemeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(a => a.Days)
                .WithOne(d => d.Adventure)
                .HasForeignKey(d => d.AdventureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.AdventureTasks)
                .WithOne(t => t.Adventure)
                .HasForeignKey(t => t.AdventureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Day>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DayNumber).IsRequired();
            entity.Property(d => d.NarrativeIntro).HasMaxLength(2000);
            entity.Property(d => d.NarrativeConvergence).HasMaxLength(2000);

            entity.HasMany(d => d.Branches)
                .WithOne(b => b.Day)
                .HasForeignKey(b => b.DayId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(d => new { d.AdventureId, d.DayNumber }).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BranchLabel).HasMaxLength(50);
            entity.Property(b => b.Order).IsRequired();

            entity.HasMany(b => b.Nodes)
                .WithOne(n => n.Branch)
                .HasForeignKey(n => n.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Order).IsRequired();
            entity.Property(n => n.NarrativeText).HasMaxLength(4000);
            entity.Property(n => n.SceneType).HasConversion<string>().HasMaxLength(20);
            entity.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(n => n.AdventureTask)
                .WithMany()
                .HasForeignKey(n => n.AdventureTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdventureTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Difficulty).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.PointValue).IsRequired();
            entity.Property(t => t.IsCompleted).IsRequired();
        });
    }
}
