using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class SavingsGoalsController : Controller
{
    private readonly AppDbContext _context;

    public SavingsGoalsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: SavingsGoals
    public async Task<IActionResult> Index()
    {
        var goals = await _context.SavingsGoals
            .Where(g => g.UserId == CurrentUserId)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();

        return View(goals);
    }

    // GET: SavingsGoals/Create
    public IActionResult Create()
    {
        return View(new SavingsGoal());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SavingsGoal goal)
    {
        goal.UserId = CurrentUserId;

        ModelState.Remove(nameof(SavingsGoal.UserId));
        ModelState.Remove(nameof(SavingsGoal.User));
        ModelState.Remove(nameof(SavingsGoal.Contributions));

        if (ModelState.IsValid)
        {
            // 1. Salva a meta primeiro para gerar o Id
            _context.Add(goal);
            await _context.SaveChangesAsync();

            // 2. Se informou saldo inicial maior que 0, debita do montante criando a transação correspondente
            if (goal.CurrentAmount > 0)
            {
                var savingsCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.UserId == CurrentUserId &&
                        (c.Name.Contains("Emergency") || c.Name.Contains("Savings") || c.Name.Contains("Reserva")));

                if (savingsCategory == null)
                {
                    savingsCategory = new Category
                    {
                        Name = "Initial Goal Allocation",
                        IsEssential = false,
                        UserId = CurrentUserId
                    };
                    _context.Categories.Add(savingsCategory);
                    await _context.SaveChangesAsync();
                }

                var initialDepositTransaction = new Transaction
                {
                    Amount = goal.CurrentAmount,
                    Date = DateTime.UtcNow,
                    Description = $"Initial Allocation: {goal.Title}",
                    Type = TransactionType.Expense, // Debita do saldo mensal disponível
                    CategoryId = savingsCategory.Id,
                    SavingsGoalId = goal.Id,
                    UserId = CurrentUserId
                };

                _context.Transactions.Add(initialDepositTransaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(goal);
    }
    // GET: SavingsGoals/Deposit/5
    // GET: SavingsGoals/Deposit/5
    public async Task<IActionResult> Deposit(int? id)
    {
        if (id == null) return NotFound();

        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);

        if (goal == null) return NotFound();

        ViewBag.GoalTitle = goal.Title;
        ViewBag.GoalId = goal.Id;

        return View();
    }


    // POST: SavingsGoals/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var goal = await _context.SavingsGoals
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);

        if (goal != null)
        {
            if (goal.Contributions.Any())
            {
                TempData["Error"] = "Não é possível excluir uma meta que possui aportes registrados. Exclua as transações vinculadas primeiro.";
                return RedirectToAction(nameof(Index));
            }

            _context.SavingsGoals.Remove(goal);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: SavingsGoals/Deposit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(int id, decimal amount, string? note)
    {
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);

        if (goal == null) return NotFound();

        if (amount <= 0)
        {
            ModelState.AddModelError("amount", "Informe um valor maior que zero.");
        }

        if (ModelState.IsValid)
        {
            // 1. Garante uma categoria para o aporte (evita violação de FK se CategoryId for obrigatório)
            var savingsCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId && (c.Name == "Poupança" || c.Name == "Savings"));

            if (savingsCategory == null)
            {
                savingsCategory = new Category
                {
                    Name = "Poupança",
                    UserId = CurrentUserId,
                    IsEssential = false
                };
                _context.Categories.Add(savingsCategory);
                await _context.SaveChangesAsync();
            }

            // 2. Atualiza o saldo acumulado da meta
            goal.CurrentAmount += amount;
            _context.Update(goal);

            // 3. Cria o lançamento contábil de saída
            var tx = new Transaction
            {
                UserId = CurrentUserId,
                Description = string.IsNullOrWhiteSpace(note) ? $"Aporte: {goal.Title}" : note,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = IrelandFinanceApp.Models.Enums.TransactionType.Expense,
                CategoryId = savingsCategory.Id, // FK preenchida com segurança
                SavingsGoalId = goal.Id
            };

            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            TempData["Success"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase)
                ? "Aporte realizado com sucesso!"
                : "Deposit completed successfully!";

            return RedirectToAction(nameof(Index));
        }

        ViewBag.GoalTitle = goal.Title;
        ViewBag.GoalId = goal.Id;
        return View();
    }
    // GET: SavingsGoals/Withdraw/5
    public async Task<IActionResult> Withdraw(int? id)
    {
        if (id == null) return NotFound();

        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);

        if (goal == null) return NotFound();

        ViewBag.GoalTitle = goal.Title;
        ViewBag.GoalId = goal.Id;
        ViewBag.MaxAmount = goal.CurrentAmount;

        return View();
    }

    // POST: SavingsGoals/Withdraw/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, decimal amount, string? note)
    {
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);

        if (goal == null) return NotFound();

        if (amount <= 0)
        {
            ModelState.AddModelError("amount", "Informe um valor maior que zero.");
        }
        else if (amount > goal.CurrentAmount)
        {
            ModelState.AddModelError("amount", $"O valor máximo para resgate é de {goal.CurrentAmount:N2}.");
        }

        if (ModelState.IsValid)
        {
            // Garante a categoria para a transação
            var savingsCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId && (c.Name == "Poupança" || c.Name == "Savings"));

            if (savingsCategory == null)
            {
                savingsCategory = new Category
                {
                    Name = "Poupança",
                    UserId = CurrentUserId,
                    IsEssential = false
                };
                _context.Categories.Add(savingsCategory);
                await _context.SaveChangesAsync();
            }

            // Reduz o saldo da meta
            goal.CurrentAmount -= amount;
            _context.Update(goal);

            // Cria a transação de entrada
            var tx = new Transaction
            {
                UserId = CurrentUserId,
                Description = string.IsNullOrWhiteSpace(note) ? $"Resgate: {goal.Title}" : note,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = IrelandFinanceApp.Models.Enums.TransactionType.Income,
                CategoryId = savingsCategory.Id,
                SavingsGoalId = goal.Id
            };

            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            TempData["Success"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase)
                ? "Resgate efetuado com sucesso!"
                : "Withdrawal completed successfully!";

            return RedirectToAction(nameof(Index));
        }

        ViewBag.GoalTitle = goal.Title;
        ViewBag.GoalId = goal.Id;
        ViewBag.MaxAmount = goal.CurrentAmount;
        return View();
    }
}