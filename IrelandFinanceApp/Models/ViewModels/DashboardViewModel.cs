using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Models.ViewModels;

public class MonthlyCashflowHistory
{
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetSavings { get; set; }
}

public class BudgetSummary
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => BudgetLimit - SpentAmount;
    public decimal PercentageUsed => BudgetLimit > 0
        ? Math.Round((SpentAmount / BudgetLimit) * 100, 1)
        : 0;
    public bool IsOverBudget => SpentAmount > BudgetLimit;
}

public class DashboardViewModel
{
    public DateTime SelectedDate { get; set; }
    public int SelectedMonth => SelectedDate.Month;
    public int SelectedYear => SelectedDate.Year;

    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyBalance => MonthlyIncome - MonthlyExpenses;

    // Métricas de Inteligência Contábil
    public decimal SavingsRate => MonthlyIncome > 0 && MonthlyBalance > 0
        ? Math.Round((MonthlyBalance / MonthlyIncome) * 100, 1)
        : 0;

    public decimal DailyBurnRate { get; set; }
    public decimal ProjectedEndOfMonthExpense { get; set; }

    public decimal EssentialMonthlyCost { get; set; }
    public decimal MonthsCovered { get; set; }

    public List<CategoryExpenseSummary> ExpensesByCategory { get; set; } = new();
    public List<BudgetSummary> BudgetSummaries { get; set; } = new();
    public List<SavingsGoal> Goals { get; set; } = new();
    public List<Transaction> RecentTransactions { get; set; } = new();

    // Histórico dos últimos 6 meses para o gráfico
    public List<MonthlyCashflowHistory> CashflowHistory { get; set; } = new();
}