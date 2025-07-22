using Application.Operations.UseCases.ProcessPayment;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Presentation.BSA;
using Presentation.BSA.Models.ViewModels;

namespace Presentation.UnitTests.Mapping;

public class MappingTests
{
    protected readonly IMapper Mapper;
    protected readonly IFixture AutoFixture;

    public MappingTests()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = new ServiceCollection();
        var startup = new Startup();
        startup.ConfigureServices(services);
        // Get the real mapper from the service provider
        // Testing the profile itself will not catch runtime errors
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        AutoFixture = new Fixture();
    }

    [Fact]
    public void CheckoutViewModel_MapsTo_ProcessPaymentCommand()
    {
        // Arrange
        var checkoutViewModel = AutoFixture.Create<CheckoutViewModel>();

        // Act
        var processPaymentCommand = Mapper.Map<ProcessPaymentCommand>(checkoutViewModel);

        // Assert
        processPaymentCommand.Should().BeEquivalentTo(checkoutViewModel);
    }
}