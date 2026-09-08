using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(150)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o valor.")]
    [Range(0.01, 99999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    [Display(Name = "Amount (€)")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Type")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    // Categoria obrigatória
    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [ValidateNever]
    public Category? Category { get; set; }

    // Vínculo opcional com a meta de reserva
    [Display(Name = "Savings Goal (Optional)")]
    public int? SavingsGoalId { get; set; }

    [ValidateNever]
    public SavingsGoal? SavingsGoal { get; set; }

    // Identificação do Usuário (preenchido via Controller)
    [ValidateNever]
    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public ApplicationUser? User { get; set; }
}