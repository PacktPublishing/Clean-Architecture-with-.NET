using Application.Interfaces.Data;
using Application.UseCases.ManageProductInventory;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class ManageProductInventoryUseCaseTests
{
    [Fact]
    public async Task UpdateProductInventoryAsync_AdministratorRole_UpdatesStockLevel()
    {
        var userId = Guid.NewGuid();
        var product = new Product("Product Name", 10.0m, 10);
        var stockLevel = 20;

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(userId)
            .Returns(new User("jdoe", "jdoe@example.com", "John Doe", new List<UserRole> { UserRole.Administrator }));

        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id)
            .Returns(product);

        var useCase = new ManageProductInventoryUseCase(userRepository, productRepository);

        await useCase.UpdateProductInventoryAsync(userId, product.Id, stockLevel);

        await productRepository.Received(1).UpdateAsync(Arg.Is<Product>(p => p.StockLevel == stockLevel));
    }

    [Fact]
    public async Task UpdateProductInventoryAsync_InvalidRole_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var product = new Product("Product Name", 10.0m, 10);
        var stockLevel = 20;

        // Simulate a user with a non-administrator role
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(userId)
            .Returns(new User("userName", "user@example.com", "John Doe", new List<UserRole> { UserRole.CustomerService }));

        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id).Returns(product);

        var useCase = new ManageProductInventoryUseCase(userRepository, productRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.UpdateProductInventoryAsync(userId, product.Id, stockLevel));

        await productRepository.Received(0).UpdateAsync(Arg.Any<Product>());
    }
}
