using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;
using OceanCard.Services;

namespace OceanCard.Pages.DeckBattle;

[Authorize]
public class IndexModel : PageModel
{
    private readonly DeckRepository _deckRepo;
    private readonly DeckBattleService _deckBattle;

    public IndexModel(DeckRepository deckRepo, DeckBattleService deckBattle)
    {
        _deckRepo = deckRepo;
        _deckBattle = deckBattle;
    }

    public List<Deck> AllDecks { get; set; } = new();

    [BindProperty] public int DeckAId { get; set; }
    [BindProperty] public int DeckBId { get; set; }

    public Deck? DeckA { get; set; }
    public Deck? DeckB { get; set; }
    public DeckBattleResult? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        AllDecks = await _deckRepo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllDecks = await _deckRepo.GetAllAsync();

        if (DeckAId == 0 || DeckBId == 0)
        {
            ErrorMessage = "請選擇兩個牌組。";
            return Page();
        }
        if (DeckAId == DeckBId)
        {
            ErrorMessage = "兩邊不能是同一個牌組。";
            return Page();
        }

        // 用 GetByIdAsync 取得含完整卡牌的牌組
        DeckA = await _deckRepo.GetByIdAsync(DeckAId);
        DeckB = await _deckRepo.GetByIdAsync(DeckBId);

        if (DeckA is null || DeckB is null)
        {
            ErrorMessage = "找不到選擇的牌組。";
            return Page();
        }
        if (DeckA.Cards.Count == 0 || DeckB.Cards.Count == 0)
        {
            ErrorMessage = "空的牌組沒辦法對戰,請先放卡進去。";
            return Page();
        }

        Result = _deckBattle.Fight(DeckA, DeckB);
        return Page();
    }
}