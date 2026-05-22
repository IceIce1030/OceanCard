using Microsoft.AspNetCore.Authentication.Cookies;
using OceanCard.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// === 第三階段新增:註冊 CardRepository ===
builder.Services.AddScoped<OceanCard.Repositories.CardRepository>();

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
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // 必須在 UseAuthorization 之前
app.UseAuthorization();

app.MapRazorPages();

app.Run();