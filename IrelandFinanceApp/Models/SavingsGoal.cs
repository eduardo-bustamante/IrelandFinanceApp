using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IrelandFinanceApp.Models;

public class SavingsGoal
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [NotMapped]
    public string Title
    {
        get => Name;
        set => Name = value;
    }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 10000000)]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000000)]
    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [NotMapped]
    public ICollection<Transaction> Contributions
    {
        get => Transactions;
        set => Transactions = value;
    }

    // Adicionado para atender Index.cshtml (linhas 59 e 93)
    [NotMapped]
    public decimal ProgressPercentage => TargetAmount > 0
        ? Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100, 1))
        : 0;
}