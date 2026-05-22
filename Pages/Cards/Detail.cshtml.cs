using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;

namespace OceanCard.Pages.Cards;

[Authorize]
public class DetailModel : PageModel
{
    private readonly CardRepository _repo;

    public DetailModel(CardRepository repo) => _repo = repo;

    public Card? Card { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Card = await _repo.GetByIdAsync(id);

        if (Card is null)
            return NotFound();   // 找不到這張卡,回 404

        return Page();
    }
}