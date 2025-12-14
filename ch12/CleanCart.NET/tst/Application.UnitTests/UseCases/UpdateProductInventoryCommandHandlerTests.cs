using Application.Interfaces.Data;
using Application.Operations.UseCases.ManageProductInventory;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class UpdateProductInventoryCommandHandlerTests
{
    [Fact]
    public async Task UpdateProductInventoryAsync_AdministratorRole_UpdatesStockLevel()
    {
        var userId = Guid.NewGuid();
        var product = new Product("Product Name", 10.0m, 10, "img.png");
        var stockLevel = 20;
        var command = new UpdateProductInventoryCommand(userId, product.Id, stockLevel);

        var userRepository = Substitute.For<IUserQueryRepository>();
        userRepository.GetByIdAsync(userId, CancellationToken.None)
            .Returns(new User("jdoe", "jdoe@example.com", "John Doe", new List<UserRole> { UserRole.Administrator }));

        var productQueryRepository = Substitute.For<IProductQueryRepository>();
        productQueryRepository.GetByIdAsync(product.Id, CancellationToken.None)
            .Returns(product);

        var productCommandRepository = Substitute.For<IProductCommandRepository>();

        var useCase = new UpdateProductInventoryCommandHandler(userRepository, productQueryRepository, productCommandRepository);

        await useCase.Handle(command, CancellationToken.None);

        await productCommandRepository.Received(1).UpdateAsync(Arg.Is<Product>(p => p.StockLevel == stockLevel), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProductInventoryAsync_InvalidRole_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var product = new Product("Product Name", 10.0m, 10, "img.png");
        var stockLevel = 20;
        var command = new UpdateProductInventoryCommand(userId, product.Id, stockLevel);

        // Simulate a user with a non-administrator role
        var userRepository = Substitute.For<IUserQueryRepository>();
        userRepository.GetByIdAsync(userId, CancellationToken.None)
            .Returns(new User("userName", "user@example.com", "John Doe", new List<UserRole> { UserRole.CustomerService }));

        var productQueryRepository = Substitute.For<IProductQueryRepository>();
        productQueryRepository.GetByIdAsync(product.Id, CancellationToken.None)
            .Returns(product);

        var productCommandRepository = Substitute.For<IProductCommandRepository>();

        var useCase = new UpdateProductInventoryCommandHandler(userRepository, productQueryRepository, productCommandRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(command, CancellationToken.None));

        await productCommandRepository.Received(0).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}