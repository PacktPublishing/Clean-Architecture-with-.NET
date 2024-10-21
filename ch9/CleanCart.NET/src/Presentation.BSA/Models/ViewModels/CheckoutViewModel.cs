namespace Presentation.BSA.Models.ViewModels;

public class CheckoutViewModel
{
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpirationMonthYear { get; set; }
    public string? CVV { get; set; }
    public string? PostalCode { get; set; }
}