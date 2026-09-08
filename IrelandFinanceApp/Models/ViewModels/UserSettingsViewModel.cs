using System.ComponentModel.DataAnnotations;

namespace IrelandFinanceApp.Models.ViewModels;

public class UserSettingsViewModel
{
    [Required]
    [Display(Name = "Display Language")]
    public string Language { get; set; } = "en"; // "en" ou "pt-BR"

    [Required]
    [Display(Name = "Currency Symbol")]
    public string CurrencySymbol { get; set; } = "€";

    [Required]
    [Display(Name = "Number & Date Culture")]
    public string CultureCode { get; set; } = "en-IE";

    [Required]
    [Display(Name = "Time Zone")]
    public string TimeZoneId { get; set; } = "Europe/Dublin";
}