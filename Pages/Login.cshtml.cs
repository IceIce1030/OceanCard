using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OceanCard.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public LoginModel(IConfiguration config) => _config = config;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // 單純顯示登入頁
    }

    public async Task<IActionResult> OnPostAsync(string password)
    {
        var correctPassword = _config["SiteAuth:Password"];

        if (password != correctPassword)
        {
            ErrorMessage = "深海守衛瞇眼看了看你,搖了搖頭。";
            return Page();
        }

        // 密碼正確:建立通行證並種下 cookie
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "OceanCardUser")
        };
        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");   // 導向首頁
    }
}