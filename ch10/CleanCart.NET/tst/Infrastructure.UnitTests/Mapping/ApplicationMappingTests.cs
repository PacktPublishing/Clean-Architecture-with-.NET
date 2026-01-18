using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.UnitTests.Mapping;

public class ApplicationMappingTests
{
    private IMapper Mapper { get; }

    public ApplicationMappingTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.core.json", optional: false)
            .Build();

        // Reuse the same extension that wires up AutoMapper and profiles
        var appStartupOrchestrator = new AppStartupOrchestrator();
        appStartupOrchestrator.Orchestrate(services, configuration);
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void DomainOrderItem_MapsTo_DomainShoppingCartItem()
    {
        var orderItem = new OrderItem(
            Guid.NewGuid(),
            "Test Product",
            10m,
            2);

        var cartItem = Mapper.Map<ShoppingCartItem>(orderItem);

        cartItem.Should().BeEquivalentTo(orderItem);
    }

    [Fact]
    public void DomainShoppingCartItem_MapsTo_DomainOrderItem()
    {
        var cartItem = new ShoppingCartItem(
            Guid.NewGuid(),
            "Test Product",
            10m,
            2);

        var orderItem = Mapper.Map<OrderItem>(cartItem);

        orderItem.Should().BeEquivalentTo(cartItem);
    }
}