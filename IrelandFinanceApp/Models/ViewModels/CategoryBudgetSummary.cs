namespace IrelandFinanceApp.Models.ViewModels;

public class CategoryBudgetSummary
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal SpentAmount { get; set; }

    public decimal PercentageUsed => BudgetLimit > 0
        ? Math.Round((SpentAmount / BudgetLimit) * 100, 1)
        : 0;

    public decimal RemainingAmount => Math.Max(0, BudgetLimit - SpentAmount);

    public bool IsOverBudget => SpentAmount > BudgetLimit;
}