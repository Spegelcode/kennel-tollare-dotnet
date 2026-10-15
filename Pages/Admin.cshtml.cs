using System.ComponentModel.DataAnnotations;
using KennelTollare.Models;
using KennelTollare.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KennelTollare.Pages;

public class AdminModel : PageModel
{
    private const string LoggedInKey = "admin-authenticated";
    private const string UsernameKey = "admin-username";
    private readonly IKennelContentStore _store;

    public new KennelContent Content { get; private set; } = new();
    public bool IsAuthenticated => HttpContext.Session.GetString(LoggedInKey) == "true";
    [BindProperty] public LoginInput Login { get; set; } = new();
    [BindProperty] public KennelContent FormContent { get; set; } = new();
    [BindProperty] public IFormFile? FemaleImage { get; set; }
    [BindProperty] public IFormFile? MaleImage { get; set; }
    [BindProperty] public IFormFile? HeroImage { get; set; }
    [BindProperty] public IFormFile? DailyImage { get; set; }
    [BindProperty] public NewLitterInput NewLitter { get; set; } = new();
    [BindProperty] public List<IFormFile> LitterImages { get; set; } = new();
    [BindProperty] public ChangePasswordInput PasswordChange { get; set; } = new();
    [TempData] public string? Message { get; set; }

    public AdminModel(IKennelContentStore store) => _store = store;

    public async Task OnGetAsync() { Content = await _store.GetAsync(); FormContent = Content; }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        if (await _store.ValidateAdminAsync(Login.Username.Trim(), Login.Password))
        {
            HttpContext.Session.SetString(LoggedInKey, "true");
            HttpContext.Session.SetString(UsernameKey, Login.Username.Trim());
            return RedirectToPage();
        }
        ModelState.AddModelError(string.Empty, "Fel användarnamn eller lösenord.");
        Content = await _store.GetAsync();
        return Page();
    }

    public IActionResult OnPostLogout() { HttpContext.Session.Clear(); return RedirectToPage(); }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (!IsAuthenticated) return RedirectToPage();
        if (!ModelState.IsValid) { Content = await _store.GetAsync(); FormContent = Content; return Page(); }

        var username = HttpContext.Session.GetString(UsernameKey);
        if (username is null || !await _store.ChangeAdminPasswordAsync(username, PasswordChange.CurrentPassword, PasswordChange.NewPassword))
        {
            ModelState.AddModelError(string.Empty, "Det nuvarande lösenordet stämmer inte.");
            Content = await _store.GetAsync();
            FormContent = Content;
            return Page();
        }

        PasswordChange = new ChangePasswordInput();
        Message = "Lösenordet är uppdaterat.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveAsync(string section)
    {
        if (!IsAuthenticated) return RedirectToPage();
        var existing = await _store.GetAsync();
        if (section == "dogs")
        {
            existing.Female = FormContent.Female;
            existing.Male = FormContent.Male;
            existing.Female.Image = await _store.SaveImageAsync(FemaleImage) ?? existing.Female.Image;
            existing.Male.Image = await _store.SaveImageAsync(MaleImage) ?? existing.Male.Image;
        }
        else
        {
            existing.Site = FormContent.Site;
            existing.Site.HeroImage = await _store.SaveImageAsync(HeroImage) ?? existing.Site.HeroImage;
            existing.Site.DailyImage = await _store.SaveImageAsync(DailyImage) ?? existing.Site.DailyImage;
        }
        await _store.SaveAsync(existing);
        Message = "Sparat! Ändringarna syns nu på hemsidan.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddLitterAsync()
    {
        if (!IsAuthenticated) return RedirectToPage();
        var content = await _store.GetAsync();
        var images = await _store.SaveImagesAsync(LitterImages);
        if (images.Count == 0) images.Add(content.Site.HeroImage);
        content.Litters.Insert(0, new Litter { Title = NewLitter.Title, Year = NewLitter.Year, Parents = NewLitter.Parents, Description = NewLitter.Description, FemalePuppies = NewLitter.FemalePuppies, MalePuppies = NewLitter.MalePuppies, Images = images, CreatedAt = DateTime.UtcNow });
        await _store.SaveAsync(content);
        Message = "Kullen är sparad och ligger överst.";
        return RedirectToPage();
    }

    public sealed class LoginInput { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
    public sealed class ChangePasswordInput
    {
        [Required] public string CurrentPassword { get; set; } = "";
        [Required, MinLength(8)] public string NewPassword { get; set; } = "";
        [Required, Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = "";
    }
    public sealed class NewLitterInput { public string Title { get; set; } = ""; public string Year { get; set; } = ""; public string Parents { get; set; } = "Iris & Loke"; public int FemalePuppies { get; set; } public int MalePuppies { get; set; } public string Description { get; set; } = ""; }
}
