using Application.Interfaces.Auth;
using Application.Operations.UseCases.AccessCustomerData;
using Application.Operations.UseCases.RemoveItemFromCart;
using Domain.Entities;
using MediatR;
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
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ShoppingCartState ShoppingCartState { get; set; } = null!;

    private User? _user;
    private ShoppingCart? _shoppingCart;

    protected override async Task OnInitializedAsync()
    {
        ShoppingCartState.OnChange += UpdateCart;
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
        if (_user != null)
        {
            var query = new AccessCustomerShoppingCartQuery(_user.Id);
            _shoppingCart = await Mediator.Send(query);
        }
    }

    private async Task RemoveItem(Guid productId)
    {
        if (_shoppingCart != null)
        {
            var command = new RemoveItemFromCartCommand(_user!.Id, productId, 1);
            await Mediator.Send(command);
            ShoppingCartState.NotifyCartChanged();
            UpdateCart();
        }
    }

    public void ProceedToCheckout()
    {
        Navigation.NavigateTo("/checkout");
    }

    public void Dispose()
    {
        ShoppingCartState.OnChange -= UpdateCart;
    }
}