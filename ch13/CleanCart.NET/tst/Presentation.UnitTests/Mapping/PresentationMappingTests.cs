using Application.Operations.UseCases.ProcessPayment;
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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder();
        var startup = new PresentationServiceComposition(config);
        startup.ConfigureServices(services);
        // Get the real mapper from the service provider
        // Testing the profile itself will not catch runtime errors
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        AutoFixture = new Fixture();
    }

    [Fact]
    public void CheckoutViewModel_MapsTo_ProcessPaymentCommand()
    {
        var checkoutViewModel = AutoFixture.Create<CheckoutViewModel>();

        var processPaymentCommand = Mapper.Map<ProcessPaymentCommand>(checkoutViewModel);

        processPaymentCommand.Should().BeEquivalentTo(checkoutViewModel);
    }
}