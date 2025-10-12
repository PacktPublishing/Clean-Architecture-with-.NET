namespace Application.Interfaces.Services.Payment;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public string ExpirationMonthYear { get; set; }
    public string CVV { get; set; }
    public string PostalCode { get; set; }
}
