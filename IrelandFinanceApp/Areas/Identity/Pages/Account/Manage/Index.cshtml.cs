using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Username { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Display(Name = "Nome Completo")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefone")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Idioma e Região")]
        public string CultureCode { get; set; } = "en-IE";

        [Required]
        [Display(Name = "Moeda")]
        public string CurrencySymbol { get; set; } = "€";

        [Required]
        [Display(Name = "Fuso Horário")]
        public string TimeZoneId { get; set; } = "Europe/Dublin";
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Username = userName ?? string.Empty;

        Input = new InputModel
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = phoneNumber,
            CultureCode = user.CultureCode ?? "en-IE",
            CurrencySymbol = user.CurrencySymbol ?? "€",
            TimeZoneId = user.TimeZoneId ?? "Europe/Dublin"
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não foi possível carregar o usuário com ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não foi possível carregar o usuário com ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Erro ao tentar salvar o número de telefone.";
                return RedirectToPage();
            }
        }

        // Atualiza as propriedades customizadas do ApplicationUser
        user.FullName = Input.FullName;
        user.CultureCode = Input.CultureCode;
        user.CurrencySymbol = Input.CurrencySymbol;
        user.TimeZoneId = Input.TimeZoneId;

        await _userManager.UpdateAsync(user);

        // Atualiza o cookie de cultura para refletir o idioma imediatamente na interface
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
        StatusMessage = "Suas informações e preferências foram atualizadas com sucesso.";
        return RedirectToPage();
    }
}