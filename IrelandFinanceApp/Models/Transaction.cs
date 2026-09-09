using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(150)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    // Forma de Pagamento
    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.DebitOrCash;

    // Vinculação com Cartão de Crédito (Anulável pois pode ser débito/dinheiro)
    public int? CreditCardId { get; set; }
    public CreditCard? CreditCard { get; set; }

    // Competência da Fatura
    public int? InvoiceMonth { get; set; }
    public int? InvoiceYear { get; set; }
    public bool IsSettled { get; set; } = false;
    public bool IsInvoicePayment { get; set; } = false;

    // Categoria
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // Meta de Poupança (Opcional)
    public int? SavingsGoalId { get; set; }
    public SavingsGoal? SavingsGoal { get; set; }

    // Dono do registro
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}