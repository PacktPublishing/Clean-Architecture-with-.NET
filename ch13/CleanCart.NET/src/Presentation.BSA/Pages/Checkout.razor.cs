using Application.Interfaces.Auth;
using Application.Operations.UseCases.AccessCustomerData;
using Application.Operations.UseCases.CalculateCartTotal;
using Application.Operations.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Presentation.BSA.Models.Validators;
using Presentation.BSA.Models.ViewModels;
using Presentation.BSA.Services;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class Checkout
{
    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private IMapper Mapper { get; set; } = null!;

    [Inject]
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ShoppingCartState ShoppingCartState { get; set; } = null!;

    private readonly CheckoutViewModel _viewModel = new();
    private readonly CheckoutViewModelValidator _viewModelValidator = new();
    private ShoppingCart? _shoppingCart;
    private User _user = null!;
    private MudForm _form = null!;
    private decimal _cartTotal;

    protected override async Task OnInitializedAsync()
    {
        _user = await AuthenticationService.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User not found.");
        var cartByUserIdQuery = new AccessCustomerShoppingCartQuery(_user.Id);
        _shoppingCart = await Mediator.Send(cartByUserIdQuery);

        if (_shoppingCart == null || !_shoppingCart.Items.Any())
        {
            Navigation.NavigateTo("/shoppingcart");
        }
        else
        {
            var cartTotalQuery = new CalculateCartTotalQuery(_user.Id);
            _cartTotal = await Mediator.Send(cartTotalQuery);
        }
    }

    private async Task OnFormSubmit()
    {
        await _form.Validate();
        Order? order = null;
        if (_form.IsValid)
        {
            try
            {
                var command = Mapper.Map<ProcessPaymentCommand>(_viewModel);
                command.UserId = _user.Id;
                command.Items = _shoppingCart!.Items.ToList();
                order = await Mediator.Send(command);
                ShoppingCartState.NotifyCartChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (order != null)
            {
                Navigation.NavigateTo($"/order-confirmation/{order.Id}");
            }
        }
    }
}