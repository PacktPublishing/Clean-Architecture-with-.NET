using Application.UseCases.ProcessPayment;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.BSA;
using Presentation.BSA.Models.ViewModels;

namespace Presentation.UnitTests.Mapping;

public class PresentationMappingTests
{
    protected readonly IMapper Mapper;
    protected readonly IFixture AutoFixture;

    public PresentationMappingTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var startup = new Startup(configuration);
        startup.ConfigureServices(services);
        // Get the real mapper from the service provider
        // Testing the profile itself will not catch runtime errors
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        AutoFixture = new Fixture();
    }

    [Fact]
    public void CheckoutViewModel_MapsTo_ProcessPaymentInput()
    {
        var checkoutViewModel = AutoFixture.Create<CheckoutViewModel>();

        var processPaymentInput = Mapper.Map<ProcessPaymentInput>(checkoutViewModel);

        processPaymentInput.Should().BeEquivalentTo(checkoutViewModel);
    }
}