using Application.UseCases.AddItemToCart;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases;

public interface IAddItemToCartUseCase
{
    Task AddItemToCartAsync(AddItemToCartInput input);
}
