using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Models.ViewModels;

public class DashboardViewModel
{
    // Período Selecionado
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow;
    public int SelectedYear => SelectedDate.Year;
    public int SelectedMonth => SelectedDate.Month;

    // Métricas do Mês / Caixa Real
    public decimal CurrentBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyExpenses => MonthlyExpense;
    public decimal MonthlyBalance => MonthlyIncome - MonthlyExpense;

    // Taxa de Poupança e Projeções
    public decimal SavingsRate => MonthlyIncome > 0 ? Math.Max(0, (MonthlyBalance / MonthlyIncome) * 100) : 0;
    public decimal DailyBurnRate { get; set; }
    public decimal ProjectedEndOfMonthExpense { get; set; }

    // Runway / Reserva de Emergência
    public decimal TotalSavings { get; set; }
    public decimal EssentialMonthlyCost { get; set; }
    public decimal MonthsCovered { get; set; }
    public decimal RunwayMonths => MonthsCovered;

    // Cartões de Crédito
    public decimal TotalCommittedCredit { get; set; }
    public decimal CurrentMonthInvoiceTotal { get; set; }
    public List<CreditCard> CreditCards { get; set; } = new();

    // Metas de Poupança (com suporte a Goals e SavingsGoals)
    public List<SavingsGoal> SavingsGoals { get; set; } = new();
    public List<SavingsGoal> Goals
    {
        get => SavingsGoals;
        set => SavingsGoals = value;
    }

    // Histórico de Fluxo de Caixa (linhas 231-233 do Index.cshtml)
    public List<CashflowMonthSummaryViewModel> CashflowHistory { get; set; } = new();

    // Categorias e Lançamentos Recentes
    public List<CategoryExpenseSummaryViewModel> ExpensesByCategory { get; set; } = new();
    public List<CategoryExpenseSummaryViewModel> CategoryExpenses
    {
        get => ExpensesByCategory;
        set => ExpensesByCategory = value;
    }
    public List<Transaction> RecentTransactions { get; set; } = new();
}

public class CategoryExpenseSummaryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class CashflowMonthSummaryViewModel
{
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetBalance => Income - Expense;
}