using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;
using OceanCard.Services;

namespace OceanCard.Pages.Battle;

[Authorize]
public class IndexModel : PageModel
{
    private readonly CardRepository _repo;
    private readonly BattleService _battle;

    public IndexModel(CardRepository repo, BattleService battle)
    {
        _repo = repo;
        _battle = battle;
    }

    public List<Card> AllCards { get; set; } = new();

    [BindProperty] public int AttackerId { get; set; }
    [BindProperty] public int DefenderId { get; set; }

    public Card? CardA { get; set; }
    public Card? CardB { get; set; }
    public DuelResult? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        AllCards = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllCards = await _repo.GetAllAsync();

        CardA = AllCards.FirstOrDefault(c => c.Id == AttackerId);
        CardB = AllCards.FirstOrDefault(c => c.Id == DefenderId);

        if (CardA is null || CardB is null)
        {
            ErrorMessage = "請選擇兩張卡牌。";
            return Page();
        }
        if (CardA.Id == CardB.Id)
        {
            ErrorMessage = "兩邊不能是同一張卡。";
            return Page();
        }

        Result = _battle.Duel(CardA, CardB);
        return Page();
    }
}