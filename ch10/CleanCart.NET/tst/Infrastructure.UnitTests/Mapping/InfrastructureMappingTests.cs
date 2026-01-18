using AutoFixture;
using AutoFixture.Kernel;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure.UnitTests.Mapping;

public class InfrastructureMappingTests
{
    IMapper Mapper { get; }
    IFixture Fixture { get; } = new Fixture().Customize(new IgnorePropertiesStartingWithNavCustomization());

    public InfrastructureMappingTests()
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
        var domainOrderItem = new OrderItem(Guid.NewGuid(), "Test Product", 20, 1);
        var expected = new ShoppingCartItem(domainOrderItem.ProductId, domainOrderItem.ProductName, domainOrderItem.ProductPrice, domainOrderItem.Quantity);

        var actual = Mapper.Map<ShoppingCartItem>(domainOrderItem);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DomainShoppingCartItem_MapsTo_DomainOrderItem()
    {
        var domainShoppingCartItem = new ShoppingCartItem(Guid.NewGuid(), "Test Product", 20, 1);
        var expected = new OrderItem(domainShoppingCartItem.ProductId, domainShoppingCartItem.ProductName, domainShoppingCartItem.ProductPrice, domainShoppingCartItem.Quantity);

        var actual = Mapper.Map<OrderItem>(domainShoppingCartItem);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DomainOrder_MapsTo_SqlOrder()
    {
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), "Test Product", 20, 1)
        };
        var domainOrder = new Order(Guid.NewGuid(), items, 20);

        var sqlOrder = Mapper.Map<Infrastructure.Persistence.Entities.Order>(domainOrder);

        sqlOrder.Should().BeEquivalentTo(domainOrder, options => options.Excluding(o => o.Status));
        sqlOrder.Status.Should().Be(domainOrder.Status);
    }

    [Fact]
    public void SqlOrder_MapsTo_DomainOrder()
    {
        var sqlOrder = Fixture.Create<Infrastructure.Persistence.Entities.Order>();

        var domainOrder = Mapper.Map<Order>(sqlOrder);

        domainOrder.Should().BeEquivalentTo(sqlOrder, options =>
        {
            options.Excluding(o => o.NavUser);
            options.Excluding(o => o.Status);
            return options;
        });
        domainOrder.Status.Should().Be(sqlOrder.Status);
    }

    [Fact]
    public void DomainProduct_MapsTo_SqlProduct()
    {
        var domainProduct = Fixture.Create<Product>();

        var sqlProduct = Mapper.Map<Infrastructure.Persistence.Entities.Product>(domainProduct);

        sqlProduct.Should().BeEquivalentTo(domainProduct);
    }

    [Fact]
    public void SqlProduct_MapsTo_DomainProduct()
    {
        var sqlProduct = Fixture.Create<Infrastructure.Persistence.Entities.Product>();

        var domainProduct = Mapper.Map<Product>(sqlProduct);

        domainProduct.Should().BeEquivalentTo(sqlProduct);
    }

    [Fact]
    public void DomainShoppingCart_MapsTo_SqlShoppingCart()
    {
        var cartItems = new List<ShoppingCartItem>
        {
            new(Guid.NewGuid(), "Test Product", 20, 1)
        };
        var domainShoppingCart = new ShoppingCart(Guid.NewGuid());
        foreach (var cartItem in cartItems)
        {
            domainShoppingCart.AddItem(cartItem.ProductId, cartItem.ProductName, cartItem.ProductPrice, cartItem.Quantity);
        }

        var sqlShoppingCart = Mapper.Map<Infrastructure.Persistence.Entities.ShoppingCart>(domainShoppingCart);

        sqlShoppingCart.Should().BeEquivalentTo(domainShoppingCart);
    }

    [Fact]
    public void SqlShoppingCart_MapsTo_DomainShoppingCart()
    {
        var sqlShoppingCart = Fixture.Create<Infrastructure.Persistence.Entities.ShoppingCart>();

        var domainShoppingCart = Mapper.Map<ShoppingCart>(sqlShoppingCart);

        domainShoppingCart.Should().BeEquivalentTo(sqlShoppingCart);
    }

    [Fact]
    public void DomainUser_MapsTo_SqlUser()
    {
        var domainUser = Fixture.Create<User>();

        var sqlUser = Mapper.Map<Infrastructure.Persistence.Entities.User>(domainUser);

        sqlUser.Should().BeEquivalentTo(domainUser);
    }

    [Fact]
    public void SqlUser_MapsTo_DomainUser()
    {
        var sqlUser = Fixture.Create<Infrastructure.Persistence.Entities.User>();
        sqlUser.Roles = [UserRole.CustomerService.ToString()];

        var domainUser = Mapper.Map<User>(sqlUser);

        domainUser.Should().BeEquivalentTo(sqlUser);
    }

    public class IgnorePropertiesStartingWithNavCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new IgnorePropertiesStartingWithNavSpecimenBuilder());
        }
    }

    public class IgnorePropertiesStartingWithNavSpecimenBuilder : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;
            if (pi != null && pi.Name.StartsWith("Nav", StringComparison.Ordinal))
            {
                return null; // Indicate that the property should not be populated
            }

            return new NoSpecimen();
        }
    }
}