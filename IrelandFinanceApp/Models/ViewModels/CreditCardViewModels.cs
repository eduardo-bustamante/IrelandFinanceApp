using System.ComponentModel.DataAnnotations;

namespace IrelandFinanceApp.Models.ViewModels;

public class CreditCardCreateEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do cartão é obrigatório.")]
    [StringLength(100)]
    [Display(Name = "Nome do Cartão")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O limite é obrigatório.")]
    [Range(0.01, 1000000, ErrorMessage = "Informe um limite válido.")]
    [Display(Name = "Limite de Crédito")]
    public decimal CreditLimit { get; set; }

    [Required(ErrorMessage = "O dia de fechamento é obrigatório.")]
    [Range(1, 31, ErrorMessage = "Dia deve estar entre 1 e 31.")]
    [Display(Name = "Dia de Fechamento")]
    public int ClosingDay { get; set; }

    [Required(ErrorMessage = "O dia de vencimento é obrigatório.")]
    [Range(1, 31, ErrorMessage = "Dia deve estar entre 1 e 31.")]
    [Display(Name = "Dia de Vencimento")]
    public int DueDay { get; set; }

    [Display(Name = "Cor do Cartão")]
    public string ColorHex { get; set; } = "#10b981";
}

public class CreditCardDetailsViewModel
{
    public CreditCard Card { get; set; } = null!;
    public int SelectedMonth { get; set; }
    public int SelectedYear { get; set; }
    public decimal TotalOpenInvoices { get; set; }
    public decimal AvailableLimit { get; set; }

    // Fatura do mês/ano selecionado
    public decimal InvoiceTotal { get; set; }
    public bool IsInvoiceSettled { get; set; }
    public List<Transaction> InvoiceTransactions { get; set; } = new();
}