using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using IrelandFinanceApp.Models;
using IrelandFinanceApp.Models.ViewModels;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SettingsController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: Settings
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var selectedLang = currentCulture.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "pt-BR" : "en";

        var model = new UserSettingsViewModel
        {
            Language = selectedLang,
            CultureCode = user.CultureCode ?? "en-IE",
            CurrencySymbol = user.CurrencySymbol ?? "€",
            TimeZoneId = user.TimeZoneId ?? "Europe/Dublin"
        };

        return View(model);
    }

    // POST: Settings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserSettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Se selecionou português, padroniza a cultura como pt-BR. Se inglês, preserva o formato escolhido (ex: en-IE ou en-US)
        user.CultureCode = model.Language.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
            ? "pt-BR"
            : model.CultureCode;

        user.CurrencySymbol = model.CurrencySymbol;
        user.TimeZoneId = model.TimeZoneId;

        // 1. Atualiza os dados na tabela AspNetUsers
        await _userManager.UpdateAsync(user);

        // 2. Regrava os Claims na sessão/cookie do usuário imediatamente
        await _signInManager.RefreshSignInAsync(user);

        // 3. Atualiza o cookie nativo do ASP.NET Core como redundância
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(user.CultureCode)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        TempData["Success"] = user.CultureCode.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
            ? "Preferências atualizadas com sucesso!"
            : "Preferences updated successfully!";

        return RedirectToAction(nameof(Index));
    }
}