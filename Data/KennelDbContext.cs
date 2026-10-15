using KennelTollare.Models;
using Microsoft.EntityFrameworkCore;

namespace KennelTollare.Data;

public sealed class KennelDbContext : DbContext
{
    public KennelDbContext(DbContextOptions<KennelDbContext> options) : base(options) { }

    public DbSet<KennelSettingsEntity> Settings => Set<KennelSettingsEntity>();
    public DbSet<DogEntity> Dogs => Set<DogEntity>();
    public DbSet<LitterEntity> Litters => Set<LitterEntity>();
    public DbSet<LitterImageEntity> LitterImages => Set<LitterImageEntity>();
    public DbSet<AdminUserEntity> AdminUsers => Set<AdminUserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KennelSettingsEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<DogEntity>().HasIndex(x => x.Role).IsUnique();
        modelBuilder.Entity<AdminUserEntity>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<LitterEntity>().HasMany(x => x.Images).WithOne().HasForeignKey(x => x.LitterId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LitterImageEntity>().HasIndex(x => new { x.LitterId, x.SortOrder });
    }
}

public sealed class KennelSettingsEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "Skogsvikens";
    public string Eyebrow { get; set; } = "";
    public string HeroTitle { get; set; } = "";
    public string HeroIntro { get; set; } = "";
    public string AboutTitle { get; set; } = "";
    public string AboutText { get; set; } = "";
    public string ContactTitle { get; set; } = "";
    public string ContactText { get; set; } = "";
    public string Email { get; set; } = "";
    public string HeroImage { get; set; } = "";
    public string DailyImage { get; set; } = "";
}

public sealed class DogEntity
{
    public int Id { get; set; }
    public string Role { get; set; } = "";
    public string Name { get; set; } = "";
    public string Year { get; set; } = "";
    public string Tags { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Image { get; set; } = "";
}

public sealed class LitterEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Year { get; set; } = "";
    public string Parents { get; set; } = "";
    public string Description { get; set; } = "";
    public int FemalePuppies { get; set; }
    public int MalePuppies { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LitterImageEntity> Images { get; set; } = new();
}

public sealed class LitterImageEntity
{
    public int Id { get; set; }
    public Guid LitterId { get; set; }
    public string Path { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class AdminUserEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
