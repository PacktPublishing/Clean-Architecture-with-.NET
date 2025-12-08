namespace Application.Interfaces.Services.Payment;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest);
    Task<PaymentStatus> GetPaymentStatusAsync(string paymentId);
}