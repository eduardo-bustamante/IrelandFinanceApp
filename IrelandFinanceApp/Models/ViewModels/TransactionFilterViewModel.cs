using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Models.ViewModels;

public class TransactionFilterViewModel
{
    // Critérios de Busca
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public TransactionType? Type { get; set; }

    // Resultados
    public List<Transaction> Transactions { get; set; } = new();

    // Totais calculados dinamicamente sobre o resultado filtrado
    public decimal FilteredIncome => Transactions
        .Where(t => t.Type == TransactionType.Income)
        .Sum(t => t.Amount);

    public decimal FilteredExpense => Transactions
        .Where(t => t.Type == TransactionType.Expense)
        .Sum(t => t.Amount);

    public decimal FilteredNet => FilteredIncome - FilteredExpense;
}