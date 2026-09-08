using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IrelandFinanceApp.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    public bool IsEssential { get; set; }

    [Range(0.00, 999999.99, ErrorMessage = "Informe um limite mensal válido.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Monthly Budget Limit (€)")]
    public decimal? MonthlyBudgetLimit { get; set; }

    [ValidateNever]
    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public ApplicationUser? User { get; set; }

    [ValidateNever]
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}