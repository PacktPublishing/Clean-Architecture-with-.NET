using Application.Interfaces.Data;
using Application.Operations.UseCases.AccessCustomerData;
using Domain.Entities;
using Moq;

namespace Application.UnitTests.Queries;

public class AccessCustomerShoppingCartQueryHandlerTests
{
    private readonly Mock<IShoppingCartQueryRepository> _mockShoppingCartQueryRepository;
    private readonly AccessCustomerShoppingCartQueryHandler _queryHandler;

    public AccessCustomerShoppingCartQueryHandlerTests()
    {
        _mockShoppingCartQueryRepository = new Mock<IShoppingCartQueryRepository>();
        _queryHandler = new AccessCustomerShoppingCartQueryHandler(_mockShoppingCartQueryRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShoppingCart_WhenUserIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        _mockShoppingCartQueryRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(shoppingCart);

        var query = new AccessCustomerShoppingCartQuery(userId);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserIdDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockShoppingCartQueryRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync((ShoppingCart?)null);

        var query = new AccessCustomerShoppingCartQuery(userId);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}