namespace Application.Interfaces.Services.Payment;

public class PaymentResult
{
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    // Other relevant properties
}