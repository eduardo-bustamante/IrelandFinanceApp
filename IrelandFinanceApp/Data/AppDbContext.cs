using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Precisão monetária decimal(18,2)
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SavingsGoal>()
            .Property(g => g.TargetAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SavingsGoal>()
            .Property(g => g.CurrentAmount)
            .HasColumnType("decimal(18,2)");

        // 2. Transação -> Usuário (Exclusão em cascata: apaga os dados ao apagar o usuário)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Categoria -> Usuário
        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. SavingsGoal -> Usuário
        modelBuilder.Entity<SavingsGoal>()
            .HasOne(s => s.User)
            .WithMany(u => u.SavingsGoals)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. SOLUÇÃO: Bloqueia caminhos concorrentes em cascata no SQL Server
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.SavingsGoal)
            .WithMany(s => s.Contributions)
            .HasForeignKey(t => t.SavingsGoalId)
            .OnDelete(DeleteBehavior.Restrict); // <-- Restrict aqui elimina o erro de múltiplos caminhos
    }
}