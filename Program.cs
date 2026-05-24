using Microsoft.AspNetCore.Authentication.Cookies;
using OceanCard.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// === 第三階段新增:註冊 CardRepository ===
builder.Services.AddScoped<OceanCard.Repositories.CardRepository>();

builder.Services.AddScoped<OceanCard.Services.BattleService>();

// === 牌組系統:註冊 DeckRepository ===
builder.Services.AddScoped<OceanCard.Repositories.DeckRepository>();

// === 牌組對戰:註冊 DeckBattleService ===
builder.Services.AddScoped<OceanCard.Services.DeckBattleService>();

// === 第二階段:Cookie 驗證 ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

DatabaseInitializer.Initialize(
    app.Configuration.GetConnectionString("Default")!);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();   // 第一階段已收藏

app.UseStatusCodePagesWithReExecute("/NotFound");
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // 必須在 UseAuthorization 之前
app.UseAuthorization();

app.MapRazorPages();

app.Run();