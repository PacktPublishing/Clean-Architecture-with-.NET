using Application.Interfaces.Services.Payment;
using Infrastructure.Clients;
using Refit;

namespace Infrastructure.Services;

public class PaymentGateway(IPaymentGatewayApi paymentGatewayApi) : IPaymentGateway
{
    private readonly IPaymentGatewayApi _paymentGatewayApi = paymentGatewayApi ?? throw new ArgumentNullException(nameof(paymentGatewayApi));

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        ApiResponse<string> result = await _paymentGatewayApi.ProcessPaymentAsync(paymentRequest);

        if (!result.IsSuccessStatusCode)
        {
            // Handle the failed request
        }

        // Mock implementation for ProcessPaymentAsync
        return new PaymentResult
        {
            TransactionId = Guid.NewGuid().ToString(),
            Status = PaymentStatus.Success,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<PaymentStatus> GetPaymentStatusAsync(string paymentId)
    {
        ApiResponse<string> result = await _paymentGatewayApi.GetPaymentStatusAsync(paymentId);

        if (!result.IsSuccessStatusCode)
        {
            // Handle the failed request
        }

        // Mock implementation for GetPaymentStatusAsync
        return PaymentStatus.Success;
    }
}