using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class CreditCardsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Injeta tanto o DbContext quanto o UserManager
    public CreditCardsController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: CreditCards
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var cards = await _context.CreditCards
            .Where(c => c.UserId == userId)
            .Include(c => c.Transactions)
            .ToListAsync();

        return View(cards);
    }

    // ... restante dos métodos (Create, Edit, Details, Delete)
}