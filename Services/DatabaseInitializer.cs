using System.Text.Json;
using KennelTollare.Data;
using KennelTollare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KennelTollare.Services;

public sealed class DatabaseInitializer
{
    private readonly KennelDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<AdminUserEntity> _passwordHasher;
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(
        KennelDbContext db,
        IWebHostEnvironment environment,
        IPasswordHasher<AdminUserEntity> passwordHasher,
        IConfiguration configuration)
    {
        _db = db;
        _environment = environment;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.Combine(_environment.ContentRootPath, "App_Data"));
        await _db.Database.EnsureCreatedAsync();

        if (!await _db.Settings.AnyAsync())
        {
            await SeedContentAsync(await LoadExistingContentAsync());
        }

        if (!await _db.AdminUsers.AnyAsync())
        {
            var user = new AdminUserEntity
            {
                Username = _configuration["Admin:Username"] ?? "admin"
            };
            var password = _configuration["Admin:Password"] ?? "tollare2027";
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            _db.AdminUsers.Add(user);
            await _db.SaveChangesAsync();
        }
    }

    private async Task<KennelContent> LoadExistingContentAsync()
    {
        var legacyPath = Path.Combine(_environment.ContentRootPath, "App_Data", "kennel-content.json");
        if (File.Exists(legacyPath))
        {
            await using var stream = File.OpenRead(legacyPath);
            var legacy = await JsonSerializer.DeserializeAsync<KennelContent>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (legacy is not null) return legacy;
        }

        return KennelContentDefaults.Create();
    }

    private async Task SeedContentAsync(KennelContent content)
    {
        _db.Settings.Add(new KennelSettingsEntity
        {
            Id = 1,
            Name = content.Site.Name,
            Eyebrow = content.Site.Eyebrow,
            HeroTitle = content.Site.HeroTitle,
            HeroIntro = content.Site.HeroIntro,
            AboutTitle = content.Site.AboutTitle,
            AboutText = content.Site.AboutText,
            ContactTitle = content.Site.ContactTitle,
            ContactText = content.Site.ContactText,
            Email = content.Site.Email,
            HeroImage = content.Site.HeroImage,
            DailyImage = content.Site.DailyImage
        });
        _db.Dogs.AddRange(ToDogEntity(content.Female, "Female"), ToDogEntity(content.Male, "Male"));

        foreach (var litter in content.Litters)
        {
            var entity = new LitterEntity
            {
                Id = litter.Id == Guid.Empty ? Guid.NewGuid() : litter.Id,
                Title = litter.Title,
                Year = litter.Year,
                Parents = litter.Parents,
                Description = litter.Description,
                FemalePuppies = litter.FemalePuppies,
                MalePuppies = litter.MalePuppies,
                CreatedAt = litter.CreatedAt == default ? DateTime.UtcNow : litter.CreatedAt,
                Images = litter.Images.Select((path, index) => new LitterImageEntity { Path = path, SortOrder = index }).ToList()
            };
            _db.Litters.Add(entity);
        }

        await _db.SaveChangesAsync();
    }

    private static DogEntity ToDogEntity(DogProfile dog, string role) => new()
    {
        Role = role,
        Name = dog.Name,
        Year = dog.Year,
        Tags = dog.Tags,
        Bio = dog.Bio,
        Image = dog.Image
    };
}
