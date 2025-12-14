using Application.Interfaces.Services.Payment;
using Infrastructure.Clients;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Infrastructure.IntegrationTests.Clients;

public class PaymentGatewayApiTests
{
    private readonly IPaymentGatewayApi _sut;

    public PaymentGatewayApiTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddCoreLayerConfiguration().Build();
        services.AddCoreLayerServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<IPaymentGatewayApi>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsPaymentResult()
    {
        var paymentRequest = new PaymentRequest();

        var result = await _sut.ProcessPaymentAsync(paymentRequest);

        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_ReturnsPaymentStatus()
    {
        var paymentId = "1";

        var result = await _sut.GetPaymentStatusAsync(paymentId);

        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}