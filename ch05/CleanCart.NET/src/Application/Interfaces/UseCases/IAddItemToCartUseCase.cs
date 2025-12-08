using Application.UseCases.AddItemToCart;

namespace Application.Interfaces.UseCases;

public interface IAddItemToCartUseCase
{
    Task AddItemToCartAsync(AddItemToCartInput input);
}
