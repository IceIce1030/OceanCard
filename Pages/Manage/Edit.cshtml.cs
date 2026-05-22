using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OceanCard.Models;
using OceanCard.Repositories;

namespace OceanCard.Pages.Manage;

[Authorize]
public class EditModel : PageModel
{
    private readonly CardRepository _repo;

    public EditModel(CardRepository repo) => _repo = repo;

    // 表單欄位(BindProperty 讓送出時自動接收)
    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Description { get; set; } = "";
    [BindProperty] public OceanElement Element { get; set; }
    [BindProperty] public Rarity Rarity { get; set; }
    [BindProperty] public int Cost { get; set; }
    [BindProperty] public int Attack { get; set; }
    [BindProperty] public int Health { get; set; }
    [BindProperty] public string Icon { get; set; } = "";   // ← 新增這行

    // 動態技能:兩個平行陣列,各列的名稱與效果
    [BindProperty] public List<string> SkillNames { get; set; } = new();
    [BindProperty] public List<string> SkillEffects { get; set; } = new();

    public bool IsEdit => Id != 0;
    public string? ErrorMessage { get; set; }

    // 進頁面:有 id 就載入該卡,沒 id 就是空白新增表單
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
            return Page();   // 新增模式

        var card = await _repo.GetByIdAsync(id.Value);
        if (card is null)
            return NotFound();

        Id = card.Id;
        Name = card.Name;
        Description = card.Description;
        Element = card.Element;
        Rarity = card.Rarity;
        Cost = card.Cost;
        Attack = card.Attack;
        Health = card.Health;
        Icon = card.ImagePath;   // ← 新增這行(ImagePath 拿來存 emoji)
        SkillNames = card.Skills.Select(s => s.Name).ToList();
        SkillEffects = card.Skills.Select(s => s.Effect).ToList();

        return Page();
    }

    // 送出表單:依有無 Id 決定新增或更新
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "卡牌名稱不能空白。";
            return Page();
        }

        // 把平行陣列組回 Skill 清單,略過名稱空白的列
        var skills = new List<Skill>();
        for (int i = 0; i < SkillNames.Count; i++)
        {
            var sName = SkillNames[i];
            if (string.IsNullOrWhiteSpace(sName))
                continue;
            var sEffect = i < SkillEffects.Count ? SkillEffects[i] : "";
            skills.Add(new Skill { Name = sName.Trim(), Effect = sEffect?.Trim() ?? "" });
        }

        var card = new Card
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description?.Trim() ?? "",
            Element = Element,
            Rarity = Rarity,
            Cost = Cost,
            Attack = Attack,
            Health = Health,
            ImagePath = Icon?.Trim() ?? "",   // ← 改這行(原本是 ImagePath = "")
            Skills = skills
        };

        if (IsEdit)
            await _repo.UpdateAsync(card);
        else
            await _repo.CreateAsync(card);

        return RedirectToPage("Index");   // 完成後回管理列表
    }
}