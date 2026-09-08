using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IrelandFinanceApp.Models;

public class SavingsGoal
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título da meta.")]
    [StringLength(100)]
    [Display(Name = "Goal Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor alvo.")]
    [Range(0.01, 99999999.99, ErrorMessage = "Informe um valor alvo válido.")]
    [Display(Name = "Target Amount (€)")]
    public decimal TargetAmount { get; set; }

    [Range(0.00, 99999999.99, ErrorMessage = "O valor inicial não pode ser negativo.")]
    [Display(Name = "Current Amount (€)")]
    public decimal CurrentAmount { get; set; } = 0.00m;

    [DataType(DataType.Date)]
    [Display(Name = "Target Date")]
    public DateTime? TargetDate { get; set; }

    // Identificação do Usuário (preenchido via Controller)
    [ValidateNever]
    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public ApplicationUser? User { get; set; }

    // Histórico de aportes associados a essa meta
    [ValidateNever]
    public ICollection<Transaction> Contributions { get; set; } = new List<Transaction>();

    // Propriedade calculada em memória (não cria coluna no banco de dados)
    public decimal ProgressPercentage =>
        TargetAmount > 0 ? Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100, 1)) : 0;
}