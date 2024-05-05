using Application.UseCases.AddItemToCart;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases
{
    public interface IAddItemToCartUseCase
    {
        Task ExecuteAsync(AddItemToCartInput input);
    }
}
