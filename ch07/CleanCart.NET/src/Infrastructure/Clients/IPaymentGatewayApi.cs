using Refit;

namespace Infrastructure.Clients;

public interface IPaymentGatewayApi
{
    // Simulate processing a payment by creating a new post
    [Post("/posts")]
    Task<ApiResponse<string>> ProcessPaymentAsync([Body]object paymentRequest);

    // Simulate retrieving payment status by getting a specific post
    [Get("/posts/{paymentId}")]
    Task<ApiResponse<string>> GetPaymentStatusAsync(string paymentId);
}