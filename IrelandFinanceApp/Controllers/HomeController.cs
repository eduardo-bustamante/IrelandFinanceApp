using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;
using IrelandFinanceApp.Models.ViewModels;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ==========================================
    // 1. DASHBOARD PRINCIPAL
    // ==========================================
    public async Task<IActionResult> Index(int? month, int? year)
    {
        var now = DateTime.UtcNow;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var selectedDate = new DateTime(selectedYear, selectedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfMonth = selectedDate;
        var endOfMonth = selectedDate.AddMonths(1).AddTicks(-1);

        // 1. Carrega todas as transações do usuário com categorias e cartões
        var allTransactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.CreditCard)
            .Where(t => t.UserId == CurrentUserId)
            .ToListAsync();

        // 2. Fluxo de Caixa Real (Apenas saídas imediatas e pagamentos de fatura afetam o saldo bancário)
        var cashTransactions = allTransactions
            .Where(t => t.PaymentMethod == PaymentMethod.DebitOrCash)
            .ToList();

        var totalIncomeAllTime = cashTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenseAllTime = cashTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var currentBalance = totalIncomeAllTime - totalExpenseAllTime;

        // Movimentações no Caixa do Mês Selecionado
        var monthlyCashTransactions = cashTransactions
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToList();

        var monthlyIncome = monthlyCashTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var monthlyExpense = monthlyCashTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // 3. Cartões de Crédito & Faturas
        var creditCards = await _context.CreditCards
            .Where(c => c.UserId == CurrentUserId)
            .Include(c => c.Transactions)
            .ToListAsync();

        var totalCommittedCredit = allTransactions
            .Where(t => t.PaymentMethod == PaymentMethod.CreditCard && !t.IsSettled)
            .Sum(t => t.Amount);

        var currentMonthInvoiceTotal = allTransactions
            .Where(t => t.PaymentMethod == PaymentMethod.CreditCard &&
                        t.InvoiceMonth == selectedMonth &&
                        t.InvoiceYear == selectedYear)
            .Sum(t => t.Amount);

        // 4. Metas de Poupança & Reserva de Emergência
        var savingsGoals = await _context.SavingsGoals
            .Where(s => s.UserId == CurrentUserId)
            .ToListAsync();

        var totalSavings = savingsGoals.Sum(s => s.CurrentAmount);

        // 5. Burn Rate & Projeção de Gastos
        var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        int daysPassed = (selectedMonth == now.Month && selectedYear == now.Year)
            ? Math.Max(1, now.Day)
            : daysInMonth;

        var dailyBurn = monthlyExpense > 0 ? Math.Round(monthlyExpense / daysPassed, 2) : 0m;
        var projectedExpense = Math.Round(dailyBurn * daysInMonth, 2);

        // Gastos essenciais do mês para estimativa de Runway
        var essentialExpenses = monthlyCashTransactions
            .Where(t => t.Type == TransactionType.Expense && t.Category != null && t.Category.IsEssential)
            .Sum(t => t.Amount);

        var baseForRunway = essentialExpenses > 0 ? essentialExpenses : (monthlyExpense > 0 ? monthlyExpense : 0m);
        var monthsCovered = baseForRunway > 0 ? Math.Round(totalSavings / baseForRunway, 1) : 0m;

        // 6. Despesas por Categoria no Mês (Soma compras de débito e cartão, sem duplicar a liquidação da fatura)
        var monthlyExpensesGrouped = allTransactions
            .Where(t => t.Type == TransactionType.Expense &&
                        !t.IsInvoicePayment &&
                        t.Date >= startOfMonth && t.Date <= endOfMonth)
            .GroupBy(t => t.Category?.Name ?? "Sem Categoria")
            .Select(g => new
            {
                Name = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .ToList();

        var totalCategoryExpenses = monthlyExpensesGrouped.Sum(x => x.Amount);

        var expensesByCategory = monthlyExpensesGrouped
            .Select(g => new CategoryExpenseSummaryViewModel
            {
                CategoryName = g.Name,
                TotalAmount = g.Amount,
                Percentage = totalCategoryExpenses > 0 ? Math.Round((g.Amount / totalCategoryExpenses) * 100, 1) : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        // 7. Histórico dos últimos 6 meses para os gráficos (CashflowHistory)
        var cashflowHistory = new List<CashflowMonthSummaryViewModel>();
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = selectedDate.AddMonths(-i);
            var startMonthTarget = new DateTime(targetDate.Year, targetDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endMonthTarget = startMonthTarget.AddMonths(1).AddTicks(-1);

            var historyTxs = cashTransactions
                .Where(t => t.Date >= startMonthTarget && t.Date <= endMonthTarget)
                .ToList();

            cashflowHistory.Add(new CashflowMonthSummaryViewModel
            {
                MonthLabel = startMonthTarget.ToString("MMM/yy", System.Globalization.CultureInfo.CurrentUICulture),
                Income = historyTxs.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expense = historyTxs.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            });
        }

        // 8. Montagem do ViewModel completo
        var viewModel = new DashboardViewModel
        {
            SelectedDate = selectedDate,
            CurrentBalance = currentBalance,
            MonthlyIncome = monthlyIncome,
            MonthlyExpense = monthlyExpense,
            DailyBurnRate = dailyBurn,
            ProjectedEndOfMonthExpense = projectedExpense,
            TotalSavings = totalSavings,
            EssentialMonthlyCost = essentialExpenses,
            MonthsCovered = monthsCovered,
            TotalCommittedCredit = totalCommittedCredit,
            CurrentMonthInvoiceTotal = currentMonthInvoiceTotal,
            CreditCards = creditCards,
            SavingsGoals = savingsGoals,
            Goals = savingsGoals,
            ExpensesByCategory = expensesByCategory,
            CashflowHistory = cashflowHistory,
            RecentTransactions = allTransactions.OrderByDescending(t => t.Date).Take(6).ToList()
        };

        return View(viewModel);
    }

    // ==========================================
    // 2. ALTERNADOR DE IDIOMA (EN / PT)
    // ==========================================
    [HttpPost]
    [AllowAnonymous]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.Lax,
                HttpOnly = true,
                Secure = Request.IsHttps
            }
        );

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    // ==========================================
    // 3. PÁGINAS AUXILIARES
    // ==========================================
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}