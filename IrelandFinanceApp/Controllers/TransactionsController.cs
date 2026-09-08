using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;
using IrelandFinanceApp.Models.ViewModels;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private bool IsPortuguese =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);

    // ==========================================
    // 1. LISTAGEM COM FILTROS AVANÇADOS
    // ==========================================
    // GET: Transactions
    public async Task<IActionResult> Index([FromQuery] TransactionFilterViewModel filter)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == CurrentUserId);

        // Filtro por texto na descrição
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Description.ToLower().Contains(term));
        }

        // Filtro por data inicial
        if (filter.StartDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(filter.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startUtc);
        }

        // Filtro por data final (até 23:59:59.999 do dia selecionado)
        if (filter.EndDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(filter.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.Date <= endUtc);
        }

        // Filtro por Categoria
        if (filter.CategoryId.HasValue && filter.CategoryId > 0)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        // Filtro por Tipo (Receita / Despesa)
        if (filter.Type.HasValue)
        {
            query = query.Where(t => t.Type == filter.Type.Value);
        }

        filter.Transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = new SelectList(categories, "Id", "Name", filter.CategoryId);

        return View(filter);
    }

    // ==========================================
    // 2. CRIAÇÃO DE LANÇAMENTO
    // ==========================================
    // GET: Transactions/Create
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesDropDownList();
        return View(new Transaction { Date = DateTime.UtcNow });
    }

    // POST: Transactions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Description,Amount,Date,Type,CategoryId")] Transaction transaction)
    {
        transaction.UserId = CurrentUserId;

        // Remove validações automáticas de navegação que possam invalidar o ModelState
        ModelState.Remove(nameof(Transaction.User));
        ModelState.Remove(nameof(Transaction.Category));
        ModelState.Remove(nameof(Transaction.UserId));

        if (ModelState.IsValid)
        {
            transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc);

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = IsPortuguese
                ? "Lançamento adicionado com sucesso!"
                : "Transaction recorded successfully!";

            return RedirectToAction(nameof(Index));
        }

        await PopulateCategoriesDropDownList(transaction.CategoryId);
        return View(transaction);
    }

    // ==========================================
    // 3. EDIÇÃO DE LANÇAMENTO
    // ==========================================
    // GET: Transactions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction == null) return NotFound();

        await PopulateCategoriesDropDownList(transaction.CategoryId);
        return View(transaction);
    }

    // POST: Transactions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Type,CategoryId,SavingsGoalId")] Transaction model)
    {
        if (id != model.Id) return NotFound();

        // Localiza a entidade original do usuário autenticado no banco
        var existingTransaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (existingTransaction == null) return NotFound();

        // Remove navegações do ModelState
        ModelState.Remove(nameof(Transaction.User));
        ModelState.Remove(nameof(Transaction.Category));
        ModelState.Remove(nameof(Transaction.UserId));

        if (ModelState.IsValid)
        {
            try
            {
                // Se a transação estiver atrelada a uma Meta (Aporte ou Resgate), 
                // ajustamos a diferença de valor na meta correspondente
                if (existingTransaction.SavingsGoalId.HasValue)
                {
                    var goal = await _context.SavingsGoals
                        .FirstOrDefaultAsync(g => g.Id == existingTransaction.SavingsGoalId && g.UserId == CurrentUserId);

                    if (goal != null)
                    {
                        decimal amountDifference = model.Amount - existingTransaction.Amount;

                        // Para depósitos/aportes (Expense), aumentar o valor aumenta a meta
                        if (model.Type == TransactionType.Expense)
                        {
                            goal.CurrentAmount += amountDifference;
                        }
                        // Para saques/resgates (Income), aumentar o saque reduz a meta
                        else if (model.Type == TransactionType.Income)
                        {
                            goal.CurrentAmount -= amountDifference;
                        }

                        if (goal.CurrentAmount < 0) goal.CurrentAmount = 0;
                    }
                }

                // Atualiza as propriedades rastreadas
                existingTransaction.Description = model.Description;
                existingTransaction.Amount = model.Amount;
                existingTransaction.Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc);
                existingTransaction.Type = model.Type;
                existingTransaction.CategoryId = model.CategoryId;

                _context.Update(existingTransaction);
                await _context.SaveChangesAsync();

                TempData["Success"] = IsPortuguese
                    ? "Lançamento atualizado com sucesso!"
                    : "Transaction updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TransactionExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        await PopulateCategoriesDropDownList(model.CategoryId);
        return View(model);
    }

    // ==========================================
    // 4. EXCLUSÃO DE LANÇAMENTO
    // ==========================================
    // GET: Transactions/Delete/5 (Confirmação opcional)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction == null) return NotFound();

        return View(transaction);
    }

    // POST: Transactions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction != null)
        {
            // Se for um lançamento automático de meta de poupança, estorna o saldo da meta
            if (transaction.SavingsGoalId.HasValue)
            {
                var goal = await _context.SavingsGoals
                    .FirstOrDefaultAsync(g => g.Id == transaction.SavingsGoalId && g.UserId == CurrentUserId);

                if (goal != null)
                {
                    if (transaction.Type == TransactionType.Expense) // Aporte cancelado
                    {
                        goal.CurrentAmount -= transaction.Amount;
                    }
                    else if (transaction.Type == TransactionType.Income) // Resgate cancelado
                    {
                        goal.CurrentAmount += transaction.Amount;
                    }

                    if (goal.CurrentAmount < 0) goal.CurrentAmount = 0;
                }
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = IsPortuguese
                ? "Lançamento removido com sucesso!"
                : "Transaction deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // MÉTODOS AUXILIARES
    // ==========================================
    private async Task PopulateCategoriesDropDownList(object? selectedCategory = null)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategory);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
    }

    private async Task<bool> TransactionExists(int id)
    {
        return await _context.Transactions.AnyAsync(e => e.Id == id && e.UserId == CurrentUserId);
    }
}