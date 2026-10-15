using KennelTollare.Data;
using KennelTollare.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace KennelTollare.Tests;

public sealed class KennelContentStoreTests
{
    [Fact]
    public async Task ContentRoundTrip_PreservesDogsLittersAndOrder()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var store = CreateStore(db);
        var content = KennelContentDefaults.Create();
        content.Litters.Insert(0, new Models.Litter
        {
            Title = "Nyaste kullen",
            Year = "2028",
            Parents = "Iris & Loke",
            FemalePuppies = 2,
            MalePuppies = 4,
            CreatedAt = DateTime.UtcNow.AddMinutes(1),
            Images = new() { "/images/test.png" }
        });

        await store.SaveAsync(content);
        var loaded = await store.GetAsync();

        Assert.Equal("Skogsvikens Iris", loaded.Female.Name);
        Assert.Equal("Rävrikets Loke", loaded.Male.Name);
        Assert.Equal("Nyaste kullen", loaded.Litters[0].Title);
        Assert.Equal(6, loaded.Litters[0].TotalPuppies);
        Assert.Equal("/images/test.png", loaded.Litters[0].Images.Single());
    }

    [Fact]
    public async Task AdminPassword_IsHashedAndCanBeChanged()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var hasher = new PasswordHasher<AdminUserEntity>();
        var user = new AdminUserEntity { Username = "admin" };
        user.PasswordHash = hasher.HashPassword(user, "initial-password");
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        var store = CreateStore(db, hasher);

        Assert.True(await store.ValidateAdminAsync("admin", "initial-password"));
        Assert.False(await store.ValidateAdminAsync("admin", "wrong-password"));
        Assert.True(await store.ChangeAdminPasswordAsync("admin", "initial-password", "new-password"));
        Assert.False(await store.ValidateAdminAsync("admin", "initial-password"));
        Assert.True(await store.ValidateAdminAsync("admin", "new-password"));
        Assert.NotEqual("new-password", (await db.AdminUsers.SingleAsync()).PasswordHash);
    }

    private static KennelDbContext CreateDb(SqliteConnection connection) => new(
        new DbContextOptionsBuilder<KennelDbContext>().UseSqlite(connection).Options);

    private static EntityKennelContentStore CreateStore(KennelDbContext db, IPasswordHasher<AdminUserEntity>? hasher = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "skogsvikens-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "wwwroot"));
        return new EntityKennelContentStore(db, new TestEnvironment(root), hasher ?? new PasswordHasher<AdminUserEntity>());
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public TestEnvironment(string root)
        {
            ContentRootPath = root;
            WebRootPath = Path.Combine(root, "wwwroot");
        }

        public string ApplicationName { get; set; } = "KennelTollare.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Test";
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
