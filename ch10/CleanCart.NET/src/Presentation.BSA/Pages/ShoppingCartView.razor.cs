using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Application.UseCases.RemoveItemFromCart;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Presentation.BSA.Services;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class ShoppingCartView : IDisposable
{
    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private IShoppingCartRepository ShoppingCartRepository { get; set; } = null!;

    [Inject]
    private IRemoveItemFromCartUseCase RemoveItemFromCartUseCase { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ShoppingCartStateContainer ShoppingCartStateContainer { get; set; } = null!;

    private User? _user;
    private ShoppingCart? _shoppingCart;

    protected override async Task OnInitializedAsync()
    {
        ShoppingCartStateContainer.OnChange += UpdateCart;
        _user ??= await AuthenticationService.GetCurrentUserAsync();
        await LoadShoppingCart();
    }

    private async void UpdateCart()
    {
        await LoadShoppingCart();
        StateHasChanged();
    }

    private async Task LoadShoppingCart()
    {
        _shoppingCart = await ShoppingCartRepository.GetByUserIdAsync(_user!.Id);
    }

    private async Task RemoveItem(Guid productId)
    {
        if (_shoppingCart != null)
        {
            var input = new RemoveItemFromCartInput(_user!.Id, productId, 1);
            await RemoveItemFromCartUseCase.RemoveItemFromCartAsync(input);
            ShoppingCartStateContainer.NotifyCartChanged();
            UpdateCart();
        }
    }

    public void ProceedToCheckout()
    {
        Navigation.NavigateTo("/checkout");
    }

    public void Dispose()
    {
        ShoppingCartStateContainer.OnChange -= UpdateCart;
    }
}