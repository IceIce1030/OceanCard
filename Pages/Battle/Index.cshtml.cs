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

    // 下拉選單用的全部卡牌
    public List<Card> AllCards { get; set; } = new();

    // 使用者選的兩張卡 Id
    [BindProperty] public int AttackerId { get; set; }
    [BindProperty] public int DefenderId { get; set; }

    // 戰鬥結果(尚未開戰時為 null)
    public AttackResult? AtoB { get; set; }
    public AttackResult? BtoA { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        AllCards = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllCards = await _repo.GetAllAsync();

        var attacker = AllCards.FirstOrDefault(c => c.Id == AttackerId);
        var defender = AllCards.FirstOrDefault(c => c.Id == DefenderId);

        if (attacker is null || defender is null)
        {
            ErrorMessage = "請選擇兩張卡牌。";
            return Page();
        }
        if (attacker.Id == defender.Id)
        {
            ErrorMessage = "兩邊不能是同一張卡。";
            return Page();
        }

        // 雙方各對對方攻擊一次
        AtoB = _battle.CalculateAttack(attacker, defender);
        BtoA = _battle.CalculateAttack(defender, attacker);

        return Page();
    }
}