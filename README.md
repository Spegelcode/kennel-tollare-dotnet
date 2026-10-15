# Skogsvikens Tollare – ASP.NET Core portfolio project

An ASP.NET Core Razor Pages kennel website written in C#. It demonstrates a public tabbed website, a protected admin area, EF Core, SQLite, secure password hashing, image uploads, automated tests, and deployment-ready publishing.

## Technology

- .NET 8 / ASP.NET Core Razor Pages
- C# and Razor (`.cshtml`)
- Entity Framework Core with SQLite
- `PasswordHasher<T>` for admin passwords
- xUnit tests with an in-memory SQLite database
- HTML, CSS, and JavaScript for the browser experience

## Run locally

From this folder:

```powershell
dotnet restore
dotnet run
```

Open the URL printed by `dotnet run`. The public website is at `/` and the editor is at `/Admin`.

On the first run, the application creates `App_Data/kennel.db`. If the old `App_Data/kennel-content.json` file exists, its content is imported into SQLite automatically.

## Admin account

The first-run demo account is:

- Username: `admin`
- Password: `tollare2027`

The password is hashed before it is stored in SQLite. Change it immediately from the Security section in `/Admin`.

For a new installation, the initial account can be configured without putting credentials in source code:

```powershell
$env:Admin__Username = "admin"
$env:Admin__Password = "use-a-strong-password"
dotnet run
```

These values are used only when the database has no admin user yet.

## Tests

Run the automated tests with:

```powershell
dotnet test Tests/KennelTollare.Tests.csproj
```

The tests cover content persistence, litter ordering, puppy totals, password verification, password changes, and the fact that plaintext passwords are not stored.

## Publish for hosting

Build a release package with:

```powershell
dotnet publish -c Release -o ./publish
```

Host the contents of `publish` on a service that supports ASP.NET Core 8. The host must allow the application to write to `App_Data` and `wwwroot/uploads`. Keep `App_Data/kennel.db` and uploaded images backed up, and configure the initial admin credentials through environment variables or the host’s secret settings.

The original static prototype remains in the sibling `kennel-tollare` folder. This folder is the server-backed portfolio version.
