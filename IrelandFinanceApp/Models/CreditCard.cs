using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IrelandFinanceApp.Models;

public class CreditCard
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do cartão é obrigatório.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 1000000)]
    public decimal CreditLimit { get; set; }

    [Range(1, 31)]
    public int ClosingDay { get; set; } // Dia de corte/fechamento da fatura

    [Range(1, 31)]
    public int DueDay { get; set; } // Dia de vencimento da fatura

    [StringLength(7)]
    public string ColorHex { get; set; } = "#10b981";

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Relacionamento com as transações deste cartão
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Calcula a competência da fatura (Mês/Ano) com base na data do gasto e no dia de corte
    /// </summary>
    public (int Month, int Year) GetInvoicePeriod(DateTime transactionDate)
    {
        if (transactionDate.Day >= ClosingDay)
        {
            var next = transactionDate.AddMonths(1);
            return (next.Month, next.Year);
        }

        return (transactionDate.Month, transactionDate.Year);
    }
}