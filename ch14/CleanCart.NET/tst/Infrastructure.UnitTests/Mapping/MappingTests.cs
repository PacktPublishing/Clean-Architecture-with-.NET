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

public class MappingTests
{
    IMapper Mapper { get; }
    IFixture Fixture { get; } = new Fixture().Customize(new IgnorePropertiesStartingWithNavCustomization());

    public MappingTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.core.json", optional: false)
            .Build();
        var appStartupOrchestrator = new AppStartupOrchestrator();
        appStartupOrchestrator.Orchestrate(services, configuration);
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void DomainOrderItem_MapsTo_DomainShoppingCartItem()
    {
        // Arrange
        var domainOrderItem = new OrderItem(Guid.NewGuid(), "Test Product", 20, 1);
        var expected = new ShoppingCartItem(domainOrderItem.ProductId, domainOrderItem.ProductName, domainOrderItem.ProductPrice, domainOrderItem.Quantity);

        // Act
        var actual = Mapper.Map<ShoppingCartItem>(domainOrderItem);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DomainShoppingCartItem_MapsTo_DomainOrderItem()
    {
        // Arrange
        var domainShoppingCartItem = new ShoppingCartItem(Guid.NewGuid(), "Test Product", 20, 1);
        var expected = new OrderItem(domainShoppingCartItem.ProductId, domainShoppingCartItem.ProductName, domainShoppingCartItem.ProductPrice, domainShoppingCartItem.Quantity);

        // Act
        var actual = Mapper.Map<OrderItem>(domainShoppingCartItem);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DomainOrder_MapsTo_SqlOrder()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), "Test Product", 20, 1)
        };
        var domainOrder = new Order(Guid.NewGuid(), items, 20);

        // Act
        var sqlOrder = Mapper.Map<Infrastructure.Persistence.Entities.Order>(domainOrder);

        // Assert
        sqlOrder.Should().BeEquivalentTo(domainOrder, options => options.Excluding(o => o.Status));
        sqlOrder.Status.Should().Be(domainOrder.Status.ToString());
    }

    [Fact]
    public void SqlOrder_MapsTo_DomainOrder()
    {
        // Arrange
        var sqlOrder = Fixture.Create<Infrastructure.Persistence.Entities.Order>();
        sqlOrder.Status = OrderStatus.Pending.ToString();

        // Act
        var domainOrder = Mapper.Map<Order>(sqlOrder);

        // Assert
        domainOrder.Should().BeEquivalentTo(sqlOrder, options =>
        {
            options.Excluding(o => o.NavUser);
            options.Excluding(o => o.Status);
            return options;
        });
        domainOrder.Status.ToString().Should().Be(sqlOrder.Status);
    }

    [Fact]
    public void DomainProduct_MapsTo_SqlProduct()
    {
        // Arrange
        var domainProduct = Fixture.Create<Product>();

        // Act
        var sqlProduct = Mapper.Map<Infrastructure.Persistence.Entities.Product>(domainProduct);

        // Assert
        sqlProduct.Should().BeEquivalentTo(domainProduct);
    }

    [Fact]
    public void SqlProduct_MapsTo_DomainProduct()
    {
        // Arrange
        var sqlProduct = Fixture.Create<Infrastructure.Persistence.Entities.Product>();

        // Act
        var domainProduct = Mapper.Map<Product>(sqlProduct);

        // Assert
        domainProduct.Should().BeEquivalentTo(sqlProduct);
    }

    [Fact]
    public void DomainShoppingCart_MapsTo_SqlShoppingCart()
    {
        // Arrange
        var items = new List<ShoppingCartItem>
        {
            new(Guid.NewGuid(), "Test Product", 20, 1)
        };
        var domainShoppingCart = new ShoppingCart(Guid.NewGuid());
        domainShoppingCart.Items.AddRange(items);

        // Act
        var sqlShoppingCart = Mapper.Map<Infrastructure.Persistence.Entities.ShoppingCart>(domainShoppingCart);

        // Assert
        sqlShoppingCart.Should().BeEquivalentTo(domainShoppingCart);
    }

    [Fact]
    public void SqlShoppingCart_MapsTo_DomainShoppingCart()
    {
        // Arrange
        var sqlShoppingCart = Fixture.Create<Infrastructure.Persistence.Entities.ShoppingCart>();

        // Act
        var domainShoppingCart = Mapper.Map<ShoppingCart>(sqlShoppingCart);

        // Assert
        domainShoppingCart.Should().BeEquivalentTo(sqlShoppingCart);
    }

    [Fact]
    public void DomainUser_MapsTo_SqlUser()
    {
        // Arrange
        var domainUser = Fixture.Create<User>();

        // Act
        var sqlUser = Mapper.Map<Infrastructure.Persistence.Entities.User>(domainUser);

        // Assert
        sqlUser.Should().BeEquivalentTo(domainUser);
    }

    [Fact]
    public void SqlUser_MapsTo_DomainUser()
    {
        // Arrange
        var sqlUser = Fixture.Create<Infrastructure.Persistence.Entities.User>();
        sqlUser.Roles = [UserRole.CustomerService.ToString()];

        // Act
        var domainUser = Mapper.Map<User>(sqlUser);

        // Assert
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