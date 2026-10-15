using KennelTollare.Models;
using KennelTollare.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KennelTollare.Pages;

public class IndexModel : PageModel
{
    private readonly IKennelContentStore _store;
    public new KennelContent Content { get; private set; } = new();
    public IReadOnlyList<Litter> Litters => Content.Litters.OrderByDescending(l => l.CreatedAt).ToList();

    public IndexModel(IKennelContentStore store) => _store = store;

    public async Task OnGetAsync() => Content = await _store.GetAsync();
}
