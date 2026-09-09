using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;
using IrelandFinanceApp.Models.ViewModels;
using IrelandFinanceApp.Models.Enums;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class CreditCardsController : Controller
{
    private readonly AppDbContext _context;

    public CreditCardsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private bool IsPortuguese =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);

    // GET: CreditCards
    public async Task<IActionResult> Index()
    {
        var cards = await _context.CreditCards
            .Where(c => c.UserId == CurrentUserId)
            .Include(c => c.Transactions)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(cards);
    }

    // GET: CreditCards/Details/5?month=9&year=2026
    public async Task<IActionResult> Details(int? id, int? month, int? year)
    {
        if (id == null) return NotFound();

        var card = await _context.CreditCards
            .Include(c => c.Transactions)
                .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (card == null) return NotFound();

        var today = DateTime.UtcNow;
        var selMonth = month ?? today.Month;
        var selYear = year ?? today.Year;

        var invoiceTxs = card.Transactions
            .Where(t => t.InvoiceMonth == selMonth && t.InvoiceYear == selYear)
            .OrderByDescending(t => t.Date)
            .ToList();

        var totalInvoice = invoiceTxs.Sum(t => t.Amount);
        var isSettled = invoiceTxs.Any() && invoiceTxs.All(t => t.IsSettled);

        // Limite comprometido = todas as despesas de faturas ainda não pagas
        var totalCommitted = card.Transactions
            .Where(t => !t.IsSettled && t.PaymentMethod == PaymentMethod.CreditCard)
            .Sum(t => t.Amount);

        var viewModel = new CreditCardDetailsViewModel
        {
            Card = card,
            SelectedMonth = selMonth,
            SelectedYear = selYear,
            TotalOpenInvoices = totalCommitted,
            AvailableLimit = Math.Max(0, card.CreditLimit - totalCommitted),
            InvoiceTotal = totalInvoice,
            IsInvoiceSettled = isSettled,
            InvoiceTransactions = invoiceTxs
        };

        return View(viewModel);
    }

    // GET: CreditCards/Create
    public IActionResult Create()
    {
        return View(new CreditCardCreateEditViewModel
        {
            ClosingDay = 20,
            DueDay = 28,
            ColorHex = "#10b981"
        });
    }

    // POST: CreditCards/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreditCardCreateEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            var card = new CreditCard
            {
                Name = model.Name,
                CreditLimit = model.CreditLimit,
                ClosingDay = model.ClosingDay,
                DueDay = model.DueDay,
                ColorHex = model.ColorHex,
                UserId = CurrentUserId
            };

            _context.CreditCards.Add(card);
            await _context.SaveChangesAsync();

            TempData["Success"] = IsPortuguese ? "Cartão cadastrado com sucesso!" : "Credit card added successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: CreditCards/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var card = await _context.CreditCards
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (card == null) return NotFound();

        var model = new CreditCardCreateEditViewModel
        {
            Id = card.Id,
            Name = card.Name,
            CreditLimit = card.CreditLimit,
            ClosingDay = card.ClosingDay,
            DueDay = card.DueDay,
            ColorHex = card.ColorHex
        };

        return View(model);
    }

    // POST: CreditCards/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreditCardCreateEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var card = await _context.CreditCards
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

            if (card == null) return NotFound();

            card.Name = model.Name;
            card.CreditLimit = model.CreditLimit;
            card.ClosingDay = model.ClosingDay;
            card.DueDay = model.DueDay;
            card.ColorHex = model.ColorHex;

            await _context.SaveChangesAsync();

            TempData["Success"] = IsPortuguese ? "Cartão atualizado com sucesso!" : "Credit card updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: CreditCards/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _context.CreditCards
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (card != null)
        {
            if (card.Transactions.Any())
            {
                TempData["Error"] = IsPortuguese
                    ? "Não é possível excluir um cartão que possui lançamentos atrelados."
                    : "Cannot delete a card with associated transactions.";
                return RedirectToAction(nameof(Index));
            }

            _context.CreditCards.Remove(card);
            await _context.SaveChangesAsync();
            TempData["Success"] = IsPortuguese ? "Cartão removido com sucesso!" : "Credit card removed successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // LIQUIDAÇÃO DE FATURA (PAGAR FATURA NO CAIXA)
    // ==========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayInvoice(int cardId, int month, int year)
    {
        var card = await _context.CreditCards
            .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == CurrentUserId);

        if (card == null) return NotFound();

        var pendingTransactions = await _context.Transactions
            .Where(t => t.CreditCardId == cardId &&
                        t.InvoiceMonth == month &&
                        t.InvoiceYear == year &&
                        !t.IsSettled)
            .ToListAsync();

        if (!pendingTransactions.Any())
        {
            TempData["Error"] = IsPortuguese ? "Não há valores pendentes nesta fatura." : "No open balance found for this invoice.";
            return RedirectToAction(nameof(Details), new { id = cardId, month, year });
        }

        var totalToPay = pendingTransactions.Sum(t => t.Amount);

        // 1. Marca os itens da fatura como quitados
        foreach (var item in pendingTransactions)
        {
            item.IsSettled = true;
        }

        // 2. Busca ou cria uma categoria de sistema "Pagamento de Fatura" para debitar do caixa
        var billCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == CurrentUserId && c.Name == "Fatura de Cartão");

        if (billCategory == null)
        {
            billCategory = new Category
            {
                Name = "Fatura de Cartão",
                IsEssential = true,
                UserId = CurrentUserId
            };
            _context.Categories.Add(billCategory);
            await _context.SaveChangesAsync();
        }

        // 3. Cria o lançamento de saída única na conta bancária (Caixa Real)
        var invoicePaymentTx = new Transaction
        {
            Description = IsPortuguese
                ? $"Pagamento de Fatura - {card.Name} ({month:D2}/{year})"
                : $"Credit Card Bill - {card.Name} ({month:D2}/{year})",
            Amount = totalToPay,
            Date = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            Type = TransactionType.Expense,
            PaymentMethod = PaymentMethod.DebitOrCash,
            CategoryId = billCategory.Id,
            IsInvoicePayment = true,
            UserId = CurrentUserId
        };

        _context.Transactions.Add(invoicePaymentTx);
        await _context.SaveChangesAsync();

        TempData["Success"] = IsPortuguese
            ? $"Fatura paga com sucesso! Saída de {totalToPay:C2} registrada no caixa."
            : $"Invoice settled successfully! {totalToPay:C2} logged as cash expense.";

        return RedirectToAction(nameof(Details), new { id = cardId, month, year });
    }
}