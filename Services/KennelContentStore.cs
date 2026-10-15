using KennelTollare.Data;
using KennelTollare.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KennelTollare.Services;

public interface IKennelContentStore
{
    Task<KennelContent> GetAsync();
    Task SaveAsync(KennelContent content);
    Task<string?> SaveImageAsync(IFormFile? file);
    Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files);
    Task<bool> ValidateAdminAsync(string username, string password);
    Task<bool> ChangeAdminPasswordAsync(string username, string currentPassword, string newPassword);
}

public sealed class EntityKennelContentStore : IKennelContentStore
{
    private readonly KennelDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<AdminUserEntity> _passwordHasher;

    public EntityKennelContentStore(
        KennelDbContext db,
        IWebHostEnvironment environment,
        IPasswordHasher<AdminUserEntity> passwordHasher)
    {
        _db = db;
        _environment = environment;
        _passwordHasher = passwordHasher;
    }

    public async Task<KennelContent> GetAsync()
    {
        var settings = await _db.Settings.AsNoTracking().SingleOrDefaultAsync();
        var dogs = await _db.Dogs.AsNoTracking().ToListAsync();
        var litters = await _db.Litters.AsNoTracking().Include(x => x.Images).OrderByDescending(x => x.CreatedAt).ToListAsync();

        if (settings is null)
        {
            return KennelContentDefaults.Create();
        }

        return new KennelContent
        {
            Site = new SiteContent
            {
                Name = settings.Name,
                Eyebrow = settings.Eyebrow,
                HeroTitle = settings.HeroTitle,
                HeroIntro = settings.HeroIntro,
                AboutTitle = settings.AboutTitle,
                AboutText = settings.AboutText,
                ContactTitle = settings.ContactTitle,
                ContactText = settings.ContactText,
                Email = settings.Email,
                HeroImage = settings.HeroImage,
                DailyImage = settings.DailyImage
            },
            Female = ToDogProfile(dogs.SingleOrDefault(x => x.Role == "Female")),
            Male = ToDogProfile(dogs.SingleOrDefault(x => x.Role == "Male")),
            Litters = litters.Select(ToLitter).ToList()
        };
    }

    public async Task SaveAsync(KennelContent content)
    {
        var settings = await _db.Settings.SingleOrDefaultAsync();
        var isNewSettings = settings is null;
        settings ??= new KennelSettingsEntity { Id = 1 };
        settings.Name = content.Site.Name;
        settings.Eyebrow = content.Site.Eyebrow;
        settings.HeroTitle = content.Site.HeroTitle;
        settings.HeroIntro = content.Site.HeroIntro;
        settings.AboutTitle = content.Site.AboutTitle;
        settings.AboutText = content.Site.AboutText;
        settings.ContactTitle = content.Site.ContactTitle;
        settings.ContactText = content.Site.ContactText;
        settings.Email = content.Site.Email;
        settings.HeroImage = content.Site.HeroImage;
        settings.DailyImage = content.Site.DailyImage;
        if (isNewSettings) _db.Settings.Add(settings);

        await UpsertDogAsync(content.Female, "Female");
        await UpsertDogAsync(content.Male, "Male");

        var existingLitters = await _db.Litters.Include(x => x.Images).ToListAsync();
        _db.LitterImages.RemoveRange(existingLitters.SelectMany(x => x.Images));
        _db.Litters.RemoveRange(existingLitters);
        _db.Litters.AddRange(content.Litters.Select(ToLitterEntity));
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ValidateAdminAsync(string username, string password)
    {
        var user = await _db.AdminUsers.SingleOrDefaultAsync(x => x.Username == username);
        if (user is null) return false;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _db.SaveChangesAsync();
        }

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public async Task<bool> ChangeAdminPasswordAsync(string username, string currentPassword, string newPassword)
    {
        var user = await _db.AdminUsers.SingleOrDefaultAsync(x => x.Username == username);
        if (user is null) return false;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (result is not (PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)) return false;

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var paths = await SaveImagesAsync(new[] { file });
        return paths.SingleOrDefault();
    }

    public async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files)
    {
        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadDirectory);
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var result = new List<string>();

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > 8 * 1024 * 1024) continue;
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) continue;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension)) continue;

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var destination = Path.Combine(uploadDirectory, fileName);
            await using var output = File.Create(destination);
            await file.CopyToAsync(output);
            result.Add($"/uploads/{fileName}");
        }

        return result;
    }

    private async Task UpsertDogAsync(DogProfile dog, string role)
    {
        var entity = await _db.Dogs.SingleOrDefaultAsync(x => x.Role == role) ?? new DogEntity { Role = role };
        entity.Name = dog.Name;
        entity.Year = dog.Year;
        entity.Tags = dog.Tags;
        entity.Bio = dog.Bio;
        entity.Image = dog.Image;
        if (entity.Id == 0) _db.Dogs.Add(entity);
    }

    private static DogProfile ToDogProfile(DogEntity? entity) => entity is null
        ? new DogProfile()
        : new DogProfile { Name = entity.Name, Year = entity.Year, Tags = entity.Tags, Bio = entity.Bio, Image = entity.Image };

    private static Litter ToLitter(LitterEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Year = entity.Year,
        Parents = entity.Parents,
        Description = entity.Description,
        FemalePuppies = entity.FemalePuppies,
        MalePuppies = entity.MalePuppies,
        CreatedAt = entity.CreatedAt,
        Images = entity.Images.OrderBy(x => x.SortOrder).Select(x => x.Path).ToList()
    };

    private static LitterEntity ToLitterEntity(Litter litter) => new()
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
}
