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
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ShoppingCartStateContainer ShoppingCartStateContainer { get; set; } = null!;

    [Inject]
    private ILogger<Checkout> Logger { get; set; } = null!;

    private readonly CheckoutViewModel _viewModel = new();
    private readonly CheckoutViewModelValidator _viewModelValidator = new();
    private ShoppingCart? _shoppingCart;
    private User _user = null!;
    private MudForm _form = null!;
    private decimal _cartTotal;
    private bool _isSubmitting;

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
        if (!_form.IsValid)
            return;

        _isSubmitting = true;

        try
        {
            var command = Mapper.Map<ProcessPaymentCommand>(_viewModel);
                command.UserId = _user.Id;
                command.Items = _shoppingCart!.Items.ToList();

            Order? order = await Mediator.Send(command);

            ShoppingCartStateContainer.NotifyCartChanged();

            if (order != null)
                Navigation.NavigateTo($"/order-confirmation/{order.Id}");
        }
        catch (Exception ex)
        {
            Snackbar.Add("An error occurred while processing your payment. Please try again.", Severity.Error);
            Logger.LogError(ex, "Error processing payment for user {UserId}", _user.Id);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}