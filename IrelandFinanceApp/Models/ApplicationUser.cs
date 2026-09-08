using Microsoft.AspNetCore.Identity;

namespace IrelandFinanceApp.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacionamentos de navegação: um usuário possui várias categorias, metas e transações
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Código da cultura para formatar número e data (ex: "en-IE", "pt-BR", "en-US")
    public string CultureCode { get; set; } = "en-IE";

    // Símbolo monetário (ex: "€", "R$", "$")
    public string CurrencySymbol { get; set; } = "€";

    // Fuso horário IANA / Windows (ex: "Europe/Dublin", "America/Sao_Paulo", "America/New_York")
    public string TimeZoneId { get; set; } = "Europe/Dublin";
}