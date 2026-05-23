using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;

namespace OceanCard.Pages.Decks;

[Authorize]
public class IndexModel : PageModel
{
    private readonly DeckRepository _repo;

    public IndexModel(DeckRepository repo) => _repo = repo;

    public List<Deck> Decks { get; set; } = new();

    [BindProperty] public string NewDeckName { get; set; } = "";

    public async Task OnGetAsync()
    {
        Decks = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewDeckName))
        {
            var id = await _repo.CreateAsync(NewDeckName.Trim());
            return RedirectToPage("Edit", new { id });
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRandomAsync()
    {
        var existing = await _repo.GetAllAsync();
        var name = $"隨機牌組 #{existing.Count + 1}";
        var id = await _repo.CreateRandomDeckAsync(name, 30);
        return RedirectToPage("Edit", new { id });
    }
}