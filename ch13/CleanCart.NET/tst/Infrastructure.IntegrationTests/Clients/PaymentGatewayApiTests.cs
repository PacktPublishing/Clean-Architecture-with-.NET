using Application.Interfaces.Services.Payment;
using Infrastructure.Clients;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Infrastructure.Startup;

namespace Infrastructure.IntegrationTests.Clients;

public class PaymentGatewayApiTests
{
    private readonly IPaymentGatewayApi _sut;

    public PaymentGatewayApiTests()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddCoreLayerConfiguration().Build();
        var appStartupOrchestrator = new AppStartupOrchestrator();
        appStartupOrchestrator.Orchestrate(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<IPaymentGatewayApi>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsPaymentResult()
    {
        // Arrange
        var paymentRequest = new PaymentRequest();

        // Act
        var result = await _sut.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_ReturnsPaymentStatus()
    {
        // Arrange
        var paymentId = "1";

        // Act
        var result = await _sut.GetPaymentStatusAsync(paymentId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}