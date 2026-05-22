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

    // 建立新牌組
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewDeckName))
        {
            var id = await _repo.CreateAsync(NewDeckName.Trim());
            return RedirectToPage("Edit", new { id });   // 建完直接進編輯
        }
        return RedirectToPage();
    }

    // 刪除牌組
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToPage();
    }
    // 一鍵隨機生成牌組
    public async Task<IActionResult> OnPostRandomAsync()
    {
        // 用現有牌組數量 +1 當編號,湊出名字
        var existing = await _repo.GetAllAsync();
        var name = $"隨機牌組 #{existing.Count + 1}";

        var id = await _repo.CreateRandomDeckAsync(name, 30);
        return RedirectToPage("Edit", new { id });   // 生成後直接進編輯頁
    }
}