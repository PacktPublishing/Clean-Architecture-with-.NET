using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests;

public class DataSeeder(IDbContextFactory<CoreDbContext> dbContextFactory, IMapper mapper)
{
    public async Task<User> SeedUser()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var user = new User($"{Guid.NewGuid()}@email.com", $"{Guid.NewGuid()}@email.com", "Test User", [UserRole.CustomerService]);
        var sqlUser = mapper.Map<Infrastructure.Persistence.Entities.User>(user);
        await dbContext.Users.AddAsync(sqlUser);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<Product> SeedProduct()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var product = new Product("Test Product", 20.99M, 100, "img.png");
        var sqlProduct = mapper.Map<Infrastructure.Persistence.Entities.Product>(product);
        await dbContext.Products.AddAsync(sqlProduct);
        await dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<Order> SeedOrder(User user, Product product)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var orderItems = new List<OrderItem>
        {
            new(product.Id, product.Name, product.Price, 1)
        };
        var order = new Order(user.Id, orderItems, 20);
        var sqlOrder = mapper.Map<Infrastructure.Persistence.Entities.Order>(order);
        await dbContext.Orders.AddAsync(sqlOrder);
        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<ShoppingCart> SeedShoppingCart(User user, Product product)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var shoppingCartItems = new List<ShoppingCartItem>
        {
            new(product.Id, product.Name, product.Price, 1)
        };
        var shoppingCart = new ShoppingCart(user.Id);
        foreach (var cartItem in shoppingCartItems)
        {
            shoppingCart.AddItem(cartItem.ProductId, cartItem.ProductName, cartItem.ProductPrice, cartItem.Quantity);
        }
        var sqlShoppingCart = mapper.Map<Infrastructure.Persistence.Entities.ShoppingCart>(shoppingCart);
        await dbContext.ShoppingCarts.AddAsync(sqlShoppingCart);
        await dbContext.SaveChangesAsync();
        return shoppingCart;
    }
}