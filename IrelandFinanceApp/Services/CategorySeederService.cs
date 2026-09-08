using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Data;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Services;

public class CategorySeederService
{
    private readonly AppDbContext _context;

    public CategorySeederService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedDefaultCategoriesForUserAsync(string userId)
    {
        var hasCategories = await _context.Categories.AnyAsync(c => c.UserId == userId);
        if (hasCategories) return;

        var defaultCategories = new List<Category>
        {
            // Receitas
            new() { Name = "Salary (Net)", IsEssential = false, UserId = userId },
            new() { Name = "Side Gig / Overtime", IsEssential = false, UserId = userId },

            // Sobrevivência / Essenciais (Irlanda)
            new() { Name = "Rent / Accommodation", IsEssential = true, UserId = userId },
            new() { Name = "Utilities (Electric / Gas)", IsEssential = true, UserId = userId },
            new() { Name = "Bins (Waste Collection)", IsEssential = true, UserId = userId },
            new() { Name = "Groceries (Lidl / Tesco / Dunnes)", IsEssential = true, UserId = userId },
            new() { Name = "Public Transport (Leap Card)", IsEssential = true, UserId = userId },
            new() { Name = "Mobile & Broadband", IsEssential = true, UserId = userId },
            new() { Name = "Health Insurance / GP", IsEssential = true, UserId = userId },

            // Lazer / Extras
            new() { Name = "Pubs & Dining Out", IsEssential = false, UserId = userId },
            new() { Name = "Travel & Weekend Trips", IsEssential = false, UserId = userId },
            new() { Name = "Shopping & Gear", IsEssential = false, UserId = userId },
            new() { Name = "Emergency Fund Transfer", IsEssential = false, UserId = userId }
        };

        _context.Categories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();
    }
}