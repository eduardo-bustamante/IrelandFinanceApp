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
    // 1. LISTAGEM COM FILTROS
    // ==========================================
    public async Task<IActionResult> Index([FromQuery] TransactionFilterViewModel filter)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.CreditCard)
            .Where(t => t.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Description.ToLower().Contains(term));
        }

        if (filter.StartDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(filter.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startUtc);
        }

        if (filter.EndDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(filter.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.Date <= endUtc);
        }

        if (filter.CategoryId.HasValue && filter.CategoryId > 0)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

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
    public async Task<IActionResult> Create()
    {
        await PopulateDropDownLists();
        return View(new Transaction
        {
            Date = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.DebitOrCash
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Description,Amount,Date,Type,PaymentMethod,CreditCardId,CategoryId")] Transaction transaction)
    {
        transaction.UserId = CurrentUserId;

        ModelState.Remove(nameof(Transaction.User));
        ModelState.Remove(nameof(Transaction.Category));
        ModelState.Remove(nameof(Transaction.CreditCard));
        ModelState.Remove(nameof(Transaction.UserId));

        // Validação de regra: compras no cartão só podem ser Despesas
        if (transaction.PaymentMethod == PaymentMethod.CreditCard)
        {
            if (!transaction.CreditCardId.HasValue || transaction.CreditCardId.Value <= 0)
            {
                ModelState.AddModelError(nameof(Transaction.CreditCardId),
                    IsPortuguese ? "Selecione o cartão de crédito utilizado." : "Please select the credit card used.");
            }

            transaction.Type = TransactionType.Expense;
        }
        else
        {
            transaction.CreditCardId = null;
            transaction.InvoiceMonth = null;
            transaction.InvoiceYear = null;
        }

        if (ModelState.IsValid)
        {
            transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc);

            // Calcula o período de fatura caso seja compra no cartão
            if (transaction.PaymentMethod == PaymentMethod.CreditCard && transaction.CreditCardId.HasValue)
            {
                var card = await _context.CreditCards
                    .FirstOrDefaultAsync(c => c.Id == transaction.CreditCardId.Value && c.UserId == CurrentUserId);

                if (card != null)
                {
                    var period = card.GetInvoicePeriod(transaction.Date);
                    transaction.InvoiceMonth = period.Month;
                    transaction.InvoiceYear = period.Year;
                    transaction.IsSettled = false;
                }
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = IsPortuguese
                ? "Lançamento adicionado com sucesso!"
                : "Transaction recorded successfully!";

            return RedirectToAction(nameof(Index));
        }

        await PopulateDropDownLists(transaction.CategoryId, transaction.CreditCardId);
        return View(transaction);
    }

    // ==========================================
    // 3. EDIÇÃO DE LANÇAMENTO
    // ==========================================
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction == null) return NotFound();

        await PopulateDropDownLists(transaction.CategoryId, transaction.CreditCardId);
        return View(transaction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Type,PaymentMethod,CreditCardId,CategoryId,SavingsGoalId,IsSettled")] Transaction model)
    {
        if (id != model.Id) return NotFound();

        var existingTransaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (existingTransaction == null) return NotFound();

        ModelState.Remove(nameof(Transaction.User));
        ModelState.Remove(nameof(Transaction.Category));
        ModelState.Remove(nameof(Transaction.CreditCard));
        ModelState.Remove(nameof(Transaction.UserId));

        if (model.PaymentMethod == PaymentMethod.CreditCard)
        {
            if (!model.CreditCardId.HasValue || model.CreditCardId.Value <= 0)
            {
                ModelState.AddModelError(nameof(Transaction.CreditCardId),
                    IsPortuguese ? "Selecione o cartão de crédito utilizado." : "Please select the credit card used.");
            }
            model.Type = TransactionType.Expense;
        }
        else
        {
            model.CreditCardId = null;
            model.InvoiceMonth = null;
            model.InvoiceYear = null;
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Ajuste de saldo em metas de poupança, se vinculado
                if (existingTransaction.SavingsGoalId.HasValue)
                {
                    var goal = await _context.SavingsGoals
                        .FirstOrDefaultAsync(g => g.Id == existingTransaction.SavingsGoalId && g.UserId == CurrentUserId);

                    if (goal != null)
                    {
                        decimal amountDifference = model.Amount - existingTransaction.Amount;
                        if (model.Type == TransactionType.Expense) goal.CurrentAmount += amountDifference;
                        else if (model.Type == TransactionType.Income) goal.CurrentAmount -= amountDifference;
                        if (goal.CurrentAmount < 0) goal.CurrentAmount = 0;
                    }
                }

                existingTransaction.Description = model.Description;
                existingTransaction.Amount = model.Amount;
                existingTransaction.Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc);
                existingTransaction.Type = model.Type;
                existingTransaction.CategoryId = model.CategoryId;
                existingTransaction.PaymentMethod = model.PaymentMethod;
                existingTransaction.CreditCardId = model.CreditCardId;

                // Recalcula o ciclo da fatura
                if (model.PaymentMethod == PaymentMethod.CreditCard && model.CreditCardId.HasValue)
                {
                    var card = await _context.CreditCards
                        .FirstOrDefaultAsync(c => c.Id == model.CreditCardId.Value && c.UserId == CurrentUserId);

                    if (card != null)
                    {
                        var period = card.GetInvoicePeriod(existingTransaction.Date);
                        existingTransaction.InvoiceMonth = period.Month;
                        existingTransaction.InvoiceYear = period.Year;
                    }
                }
                else
                {
                    existingTransaction.InvoiceMonth = null;
                    existingTransaction.InvoiceYear = null;
                    existingTransaction.IsSettled = false;
                }

                _context.Update(existingTransaction);
                await _context.SaveChangesAsync();

                TempData["Success"] = IsPortuguese
                    ? "Lançamento atualizado com sucesso!"
                    : "Transaction updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Transactions.AnyAsync(e => e.Id == model.Id && e.UserId == CurrentUserId))
                    return NotFound();
                throw;
            }
        }

        await PopulateDropDownLists(model.CategoryId, model.CreditCardId);
        return View(model);
    }

    // ==========================================
    // 4. EXCLUSÃO DE LANÇAMENTO
    // ==========================================
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction != null)
        {
            if (transaction.SavingsGoalId.HasValue)
            {
                var goal = await _context.SavingsGoals
                    .FirstOrDefaultAsync(g => g.Id == transaction.SavingsGoalId && g.UserId == CurrentUserId);

                if (goal != null)
                {
                    if (transaction.Type == TransactionType.Expense) goal.CurrentAmount -= transaction.Amount;
                    else if (transaction.Type == TransactionType.Income) goal.CurrentAmount += transaction.Amount;
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

    private async Task PopulateDropDownLists(object? selectedCategory = null, object? selectedCard = null)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var cards = await _context.CreditCards
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategory);
        ViewBag.CreditCardId = new SelectList(cards, "Id", "Name", selectedCard);
        ViewBag.HasCreditCards = cards.Any();
    }
}