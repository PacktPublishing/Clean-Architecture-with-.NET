using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;

namespace Application.UseCases.CalculateCartTotal;

public class CalculateCartTotalUseCase(IShoppingCartRepository shoppingCartRepository) : ICalculateCartTotalUseCase
{
    public async Task<decimal> CalculateTotalAsync(CalculateCartTotalInput input)
    {
        ShoppingCart? shoppingCart = await shoppingCartRepository.GetByUserIdAsync(input.UserId);

        if (shoppingCart == null)
        {
            return 0;
        }

        decimal subtotal = CalculateSubtotal(shoppingCart);
        decimal taxes = CalculateTaxes(subtotal);
        decimal total = subtotal + taxes;

        return total;
    }

    private static decimal CalculateSubtotal(ShoppingCart shoppingCart)
    {
        return shoppingCart.Items.Sum(item => item.ProductPrice * item.Quantity);
    }

    private static decimal CalculateTaxes(decimal subtotal)
    {
        // Implement tax calculation logic here based on business rules.
        // For simplicity, we assume a flat tax rate in this example.
        const decimal taxRate = 0.08M;
        return subtotal * taxRate;
    }
}