using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class SavingsGoalsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SavingsGoalsController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: SavingsGoals
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var goals = await _context.SavingsGoals
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.TargetDate)
            .ToListAsync();

        return View(goals);
    }

    // GET: SavingsGoals/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var savingsGoal = await _context.SavingsGoals
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (savingsGoal == null) return NotFound();

        return View(savingsGoal);
    }

    // GET: SavingsGoals/Create
    public IActionResult Create()
    {
        var model = new SavingsGoal
        {
            TargetDate = DateTime.Today.AddMonths(6)
        };
        return View(model);
    }

    // POST: SavingsGoals/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,TargetAmount,CurrentAmount,TargetDate")] SavingsGoal savingsGoal)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        // Associa o Id do usuário logado
        savingsGoal.UserId = userId;

        // Remove validações de propriedades que não vêm do formulário HTML
        ModelState.Remove(nameof(savingsGoal.UserId));
        ModelState.Remove(nameof(savingsGoal.User));
        ModelState.Remove(nameof(savingsGoal.Transactions));

        // Se houver valor inicial não preenchido, define como 0
        if (savingsGoal.CurrentAmount < 0)
        {
            savingsGoal.CurrentAmount = 0;
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.SavingsGoals.Add(savingsGoal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Meta criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Captura erro de banco de dados se ocorrer
                ModelState.AddModelError(string.Empty, $"Erro ao gravar no banco de dados: {ex.Message}");
            }
        }

        // Se falhar a validação, este bloco ajuda a ver na tela o que travou
        return View(savingsGoal);
    }
    // GET: SavingsGoals/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var savingsGoal = await _context.SavingsGoals
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (savingsGoal == null) return NotFound();

        return View(savingsGoal);
    }

    // POST: SavingsGoals/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TargetAmount,CurrentAmount,TargetDate")] SavingsGoal savingsGoal)
    {
        if (id != savingsGoal.Id) return NotFound();

        var userId = _userManager.GetUserId(User);
        var existingGoal = await _context.SavingsGoals
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (existingGoal == null) return NotFound();

        ModelState.Remove(nameof(savingsGoal.UserId));
        ModelState.Remove(nameof(savingsGoal.User));

        if (ModelState.IsValid)
        {
            try
            {
                existingGoal.Name = savingsGoal.Name;
                existingGoal.TargetAmount = savingsGoal.TargetAmount;
                existingGoal.CurrentAmount = savingsGoal.CurrentAmount;
                existingGoal.TargetDate = savingsGoal.TargetDate;

                _context.Update(existingGoal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meta atualizada com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SavingsGoalExists(savingsGoal.Id, userId!))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(savingsGoal);
    }

    // GET: SavingsGoals/Deposit/5
    public async Task<IActionResult> Deposit(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (goal == null) return NotFound();

        return View(goal);
    }

    // POST: SavingsGoals/Deposit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["ErrorMessage"] = "Informe um valor de aporte positivo.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User);
        var goal = await _context.SavingsGoals
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (goal == null) return NotFound();

        goal.CurrentAmount += amount;
        _context.Update(goal);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Aporte de {amount:C} realizado com sucesso para '{goal.Name}'!";
        return RedirectToAction(nameof(Index));
    }
    // GET: SavingsGoals/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var savingsGoal = await _context.SavingsGoals
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (savingsGoal == null) return NotFound();

        return View(savingsGoal);
    }

    // POST: SavingsGoals/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);
        var savingsGoal = await _context.SavingsGoals
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (savingsGoal != null)
        {
            if (savingsGoal.Transactions != null && savingsGoal.Transactions.Any())
            {
                TempData["ErrorMessage"] = "Esta meta não pode ser excluída pois possui lançamentos associados.";
                return RedirectToAction(nameof(Index));
            }

            _context.SavingsGoals.Remove(savingsGoal);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Meta removida com sucesso!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool SavingsGoalExists(int id, string userId)
    {
        return _context.SavingsGoals.Any(e => e.Id == id && e.UserId == userId);
    }
}