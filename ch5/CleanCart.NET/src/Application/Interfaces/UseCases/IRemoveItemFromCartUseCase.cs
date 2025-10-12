using Application.UseCases.RemoveItemFromCart;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases;

public interface IRemoveItemFromCartUseCase
{
    Task RemoveItemFromCartAsync(RemoveItemFromCartInput input);
}
