using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Areas.Identity.Pages.Account.Manage;

public class PreferencesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PreferencesModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Idioma e Região")]
        public string CultureCode { get; set; } = "en-IE";

        [Required]
        [Display(Name = "Moeda Padrão")]
        public string CurrencySymbol { get; set; } = "€";

        [Required]
        [Display(Name = "Fuso Horário")]
        public string TimeZoneId { get; set; } = "Europe/Dublin";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        Input = new InputModel
        {
            CultureCode = user.CultureCode ?? "en-IE",
            CurrencySymbol = user.CurrencySymbol ?? "€",
            TimeZoneId = user.TimeZoneId ?? "Europe/Dublin"
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.CultureCode = Input.CultureCode;
        user.CurrencySymbol = Input.CurrencySymbol;
        user.TimeZoneId = Input.TimeZoneId;

        await _userManager.UpdateAsync(user);

        var shortCulture = Input.CultureCode.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "pt" : "en";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(shortCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.Lax,
                HttpOnly = true,
                Secure = Request.IsHttps
            }
        );

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Preferências regionais atualizadas com sucesso.";
        return RedirectToPage();
    }
}