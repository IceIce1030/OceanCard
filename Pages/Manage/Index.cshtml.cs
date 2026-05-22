using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;

namespace OceanCard.Pages.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly CardRepository _repo;

    public IndexModel(CardRepository repo) => _repo = repo;

    public List<Card> Cards { get; set; } = new();

    public async Task OnGetAsync()
    {
        Cards = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToPage();
    }
}