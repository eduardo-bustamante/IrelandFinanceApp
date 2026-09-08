using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models.Enums;
using IrelandFinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var now = DateTime.UtcNow;
        var selectedDate = new DateTime(year ?? now.Year, month ?? now.Month, 1);
        var nextMonthDate = selectedDate.AddMonths(1);

        // Lançamentos do mês selecionado
        var monthTransactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == CurrentUserId && t.Date >= selectedDate && t.Date < nextMonthDate)
            .ToListAsync();

        var income = monthTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var expenses = monthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // Métricas de Burn Rate e Projeção
        int daysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
        int daysElapsed = (selectedDate.Year == now.Year && selectedDate.Month == now.Month)
            ? Math.Max(1, now.Day)
            : daysInMonth;

        decimal dailyBurn = daysElapsed > 0 ? expenses / daysElapsed : 0;
        decimal projectedExpense = dailyBurn * daysInMonth;

        // Distribuição por categoria
        var expensesByCategory = monthTransactions
            .Where(t => t.Type == TransactionType.Expense && t.Category != null)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new CategoryExpenseSummary
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(g => g.TotalAmount)
            .ToList();

        // Metas e Runway
        var goals = await _context.SavingsGoals
            .Where(g => g.UserId == CurrentUserId)
            .ToListAsync();

        var totalSaved = goals.Sum(g => g.CurrentAmount);

        var essentialMonthlyCost = await _context.Categories
            .Where(c => c.UserId == CurrentUserId && c.IsEssential && c.MonthlyBudgetLimit.HasValue)
            .SumAsync(c => c.MonthlyBudgetLimit.Value);

        if (essentialMonthlyCost == 0)
        {
            // Fallback para os gastos essenciais realizados no mês
            essentialMonthlyCost = monthTransactions
                .Where(t => t.Type == TransactionType.Expense && t.Category != null && t.Category.IsEssential)
                .Sum(t => t.Amount);
        }

        var monthsCovered = essentialMonthlyCost > 0
            ? Math.Round(totalSaved / essentialMonthlyCost, 1)
            : (totalSaved > 0 ? 99 : 0);

        // Histórico dos últimos 6 meses (para o gráfico de evolução)
        var sixMonthsAgo = selectedDate.AddMonths(-5);
        var historicalTxs = await _context.Transactions
            .Where(t => t.UserId == CurrentUserId && t.Date >= sixMonthsAgo && t.Date < nextMonthDate)
            .ToListAsync();

        var cashflowHistory = new List<MonthlyCashflowHistory>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = selectedDate.AddMonths(-i);
            var nextTarget = targetMonth.AddMonths(1);

            var txs = historicalTxs.Where(t => t.Date >= targetMonth && t.Date < nextTarget).ToList();
            var mIncome = txs.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var mExpense = txs.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            cashflowHistory.Add(new MonthlyCashflowHistory
            {
                MonthLabel = targetMonth.ToString("MMM/yy", System.Globalization.CultureInfo.CurrentUICulture),
                Income = mIncome,
                Expense = mExpense,
                NetSavings = mIncome - mExpense
            });
        }

        var model = new DashboardViewModel
        {
            SelectedDate = selectedDate,
            MonthlyIncome = income,
            MonthlyExpenses = expenses,
            DailyBurnRate = dailyBurn,
            ProjectedEndOfMonthExpense = projectedExpense,
            EssentialMonthlyCost = essentialMonthlyCost,
            MonthsCovered = monthsCovered,
            ExpensesByCategory = expensesByCategory,
            Goals = goals,
            CashflowHistory = cashflowHistory,
            RecentTransactions = monthTransactions.OrderByDescending(t => t.Date).Take(7).ToList()
        };

        return View(model);
    }

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
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}