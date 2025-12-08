using Application.Interfaces.Data;
using Application.UseCases.AccessCustomerData;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class AccessCustomerDataUseCaseTests
{
    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetCustomerCart_ValidInput_IsAuthorized_ReturnsCustomerCart(UserRole role)
    {
        // Arrange
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(requestingUserId)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockShoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        mockShoppingCartRepository.GetByUserIdAsync(targetUserId)
            .Returns(new ShoppingCart(targetUserId));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            mockShoppingCartRepository,
            mockUserRepository);

        // Act
        var customerCart = await useCase.GetCustomerCartAsync(requestingUserId, targetUserId);

        // Assert
        Assert.NotNull(customerCart);
    }

    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetOrderHistory_ValidInput_IsAuthorized_ReturnsCustomerOrderHistory(UserRole role)
    {
        // Arrange
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(requestingUserId)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.GetOrdersByUserIdAsync(targetUserId)
            .Returns(new List<Order>());

        var useCase = new AccessCustomerDataUseCase(
            mockOrderRepository,
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        // Act
        var orderHistory = await useCase.GetOrderHistoryAsync(requestingUserId, targetUserId);

        // Assert
        Assert.NotNull(orderHistory);
        Assert.Empty(orderHistory);
    }

    [Fact]
    public async Task GetCustomerCart_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(targetUserId)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetCustomerCartAsync(requestingUserId, targetUserId));
    }

    [Fact]
    public async Task GetOrderHistory_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var requestingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(requestingUserId)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetOrderHistoryAsync(requestingUserId, targetUserId));
    }
}