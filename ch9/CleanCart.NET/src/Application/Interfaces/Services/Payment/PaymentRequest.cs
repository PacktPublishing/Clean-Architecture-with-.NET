namespace Application.Interfaces.Services.Payment;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationMonthYear { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}