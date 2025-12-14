using Application.Interfaces.Data;
using Application.Operations.UseCases.AccessCustomerData;
using Domain.Entities;
using NSubstitute;

namespace Application.UnitTests.Queries;

public class AccessCustomerShoppingCartQueryHandlerTests
{
    private readonly IShoppingCartQueryRepository _mockShoppingCartQueryRepository;
    private readonly AccessCustomerShoppingCartQueryHandler _queryHandler;

    public AccessCustomerShoppingCartQueryHandlerTests()
    {
        _mockShoppingCartQueryRepository = Substitute.For<IShoppingCartQueryRepository>();
        _queryHandler = new AccessCustomerShoppingCartQueryHandler(_mockShoppingCartQueryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnShoppingCart_WhenUserIdExists()
    {
        var userId = Guid.NewGuid();
        var shoppingCart = new ShoppingCart(userId);
        _mockShoppingCartQueryRepository.GetByUserIdAsync(userId)
            .Returns(shoppingCart);

        var query = new AccessCustomerShoppingCartQuery(userId);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserIdDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _mockShoppingCartQueryRepository.GetByUserIdAsync(userId)
            .Returns((ShoppingCart?)null);

        var query = new AccessCustomerShoppingCartQuery(userId);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }
}