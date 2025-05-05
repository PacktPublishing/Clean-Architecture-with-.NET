using System.Net;
using Application.Interfaces.Services.Payment;
using Infrastructure.Clients;
using Infrastructure.Services;
using Moq;
using Refit;

namespace Infrastructure.UnitTests.Services;

public class PaymentGatewayTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsPaymentResult()
    {
        // Arrange
        var mockApi = new Mock<IPaymentGatewayApi>();
        var mockApiResponse = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.Created), string.Empty, new RefitSettings());
        mockApi.Setup(x => x.ProcessPaymentAsync(It.IsAny<object>())).ReturnsAsync(mockApiResponse);
        var paymentRequest = new PaymentRequest();
        var paymentGateway = new PaymentGateway(mockApi.Object);

        // Act
        var result = await paymentGateway.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.TransactionId);
        Assert.Equal(PaymentStatus.Success, result.Status);
        Assert.True(result.Timestamp <= DateTime.UtcNow);
        mockApi.Verify(x => x.ProcessPaymentAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_ReturnsPaymentStatus()
    {
        // Arrange
        var mockApi = new Mock<IPaymentGatewayApi>();
        var mockApiResponse = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.OK), string.Empty, new RefitSettings());
        mockApi.Setup(x => x.GetPaymentStatusAsync(It.IsAny<string>())).ReturnsAsync(mockApiResponse);
        var paymentId = "paymentId";
        var paymentGateway = new PaymentGateway(mockApi.Object);

        // Act
        var result = await paymentGateway.GetPaymentStatusAsync(paymentId);

        // Assert
        Assert.Equal(PaymentStatus.Success, result);
        mockApi.Verify(x => x.GetPaymentStatusAsync(It.IsAny<string>()), Times.Once);
    }
}