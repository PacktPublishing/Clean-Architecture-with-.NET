using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Application.UseCases.CalculateCartTotal;
using Application.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
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
    private IShoppingCartRepository ShoppingCartRepository { get; set; } = null!;

    [Inject]
    private ICalculateCartTotalUseCase CalculateCartTotalUseCase { get; set; } = null!;

    [Inject]
    private IProcessPaymentUseCase ProcessPaymentUseCase { get; set; } = null!;

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
        _shoppingCart = await ShoppingCartRepository.GetByUserIdAsync(_user.Id);

        if (_shoppingCart == null || !_shoppingCart.Items.Any())
        {
            Navigation.NavigateTo("/shoppingcart");
        }
        else
        {
            var input = new CalculateCartTotalInput(_user.Id);
            _cartTotal = await CalculateCartTotalUseCase.CalculateTotalAsync(input);
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
                var paymentInput = Mapper.Map<ProcessPaymentInput>(_viewModel);
                paymentInput.UserId = _user.Id;
                paymentInput.Items = _shoppingCart!.Items.ToList();
                order = await ProcessPaymentUseCase.ProcessPaymentAsync(paymentInput);
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