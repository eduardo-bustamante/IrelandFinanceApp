using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<SavingsGoal> SavingsGoals { get; set; } = null!;
    public DbSet<CreditCard> CreditCards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // 1. RELACIONAMENTOS COM USUÁRIO (Identity)
        // ==========================================
        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavingsGoal>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CreditCard>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // 2. RELACIONAMENTOS DA TRANSAÇÃO (Sem Ciclos)
        // Todos com Restrict para o SQL Server não reclamar de múltiplos caminhos
        // ==========================================

        // Transação -> Categoria
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Transação -> Meta de Poupança (ALTERADO PARA RESTRICT)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.SavingsGoal)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SavingsGoalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Transação -> Cartão de Crédito
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.CreditCard)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==========================================
        // 3. PRECISÕES DECIMAIS
        // ==========================================
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Category>()
            .Property(c => c.MonthlyBudgetLimit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(s => s.TargetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(s => s.CurrentAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CreditCard>()
            .Property(c => c.CreditLimit)
            .HasPrecision(18, 2);
    }
}