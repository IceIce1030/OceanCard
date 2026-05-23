using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;

namespace OceanCard.Pages.Decks;

[Authorize]
public class EditModel : PageModel
{
    private readonly DeckRepository _deckRepo;
    private readonly CardRepository _cardRepo;

    public EditModel(DeckRepository deckRepo, CardRepository cardRepo)
    {
        _deckRepo = deckRepo;
        _cardRepo = cardRepo;
    }

    public Deck? Deck { get; set; }
    public List<Card> AllCards { get; set; } = new();   // 卡庫(全部卡)

    // 載入:這個牌組 + 全部卡牌
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Deck = await _deckRepo.GetByIdAsync(id);
        if (Deck is null)
            return NotFound();

        AllCards = await _cardRepo.GetAllAsync();
        return Page();
    }

    // 加一張卡進牌組
    public async Task<IActionResult> OnPostAddAsync(int id, int cardId)
    {
        await _deckRepo.AddCardAsync(id, cardId);
        return RedirectToPage(new { id });
    }

    // 從牌組移除一張卡
    public async Task<IActionResult> OnPostRemoveAsync(int id, int cardId)
    {
        await _deckRepo.RemoveCardAsync(id, cardId);
        return RedirectToPage(new { id });
    }
}