using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories); // Passa List<Category> para Views/Categories/Index.cshtml
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View(new Category()); // Passa Category individual para Views/Categories/Create.cshtml
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        category.UserId = CurrentUserId;
        ModelState.Remove("UserId");

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        // Garante que o UserId continue sendo o do usuário logado
        category.UserId = CurrentUserId;
        ModelState.Remove("UserId");

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            // Verifica se a categoria pertence de fato a este usuário antes de atualizar
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == id && c.UserId == CurrentUserId);

            if (!categoryExists)
            {
                return NotFound();
            }

            _context.Update(category);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(category.Id))
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

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id && e.UserId == CurrentUserId);
    }
}