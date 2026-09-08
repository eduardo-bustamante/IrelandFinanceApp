using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Configuração do Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>();

// 3. Configuração dos Cookies de Sessão / Login para Rede Local (HTTP)
builder.Services.ConfigureApplicationCookie(options =>
{
    // Permite que o cookie funcione tanto em HTTP quanto em HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// 4. Suporte a Controllers, Views e Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 5. Configuração de Idiomas / Localização (EN e PT)
var supportedCultures = new[]
{
    new CultureInfo("en-IE"),
    new CultureInfo("en-US"),
    new CultureInfo("en"),
    new CultureInfo("pt-BR"),
    new CultureInfo("pt")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

var app = builder.Build();

// 6. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// IMPORTANTE: Desativado para permitir acesso local via HTTP sem erro de SSL no celular
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Localização logo após Routing e antes de Autenticação
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

// 7. Mapeamento de Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();