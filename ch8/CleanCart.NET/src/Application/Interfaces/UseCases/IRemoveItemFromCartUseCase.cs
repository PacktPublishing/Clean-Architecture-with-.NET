using Application.UseCases.RemoveItemFromCart;

namespace Application.Interfaces.UseCases;

public interface IRemoveItemFromCartUseCase
{
    Task RemoveItemFromCartAsync(RemoveItemFromCartInput input);
}