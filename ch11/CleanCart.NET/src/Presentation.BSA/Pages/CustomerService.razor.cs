using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Application.UseCases.RemoveItemFromCart;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Presentation.BSA.Pages;

[Authorize(Policy = "CustomerService")]
public partial class CustomerService
{
    private Severity _severity = Severity.Info;
    private string _severityText = "Search for a customer's cart or order history.";
    private string _customerId = string.Empty;
    private bool _showShoppingCart;
    private bool _showOrderHistory;
    private User? _customer;
    private User? _currentUser;
    private ShoppingCart? _shoppingCart;
    private List<Order> _orderHistory = new();

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private IUserRepository UserRepository { get; set; } = null!;

    [Inject]
    private IAccessCustomerDataUseCase AccessCustomerDataUseCase { get; set; } = null!;

    [Inject]
    private IRemoveItemFromCartUseCase RemoveItemFromCartUseCase { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _currentUser = await AuthenticationService.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User not found.");
    }

    private async Task LoadCustomer()
    {
        if (!Guid.TryParse(_customerId, out var customerIdGuid))
        {
            _shoppingCart = null;
            _customer = null;
        }

        _customer = await UserRepository.GetByIdAsync(customerIdGuid);
    }

    private async Task LoadCart(string customerId)
    {
        _customerId = customerId;
        // Reset the cart for the new customer
        _shoppingCart = null;
        await LoadCustomer();

        if (_customer != null)
        {
            _shoppingCart = await AccessCustomerDataUseCase.GetCustomerCartAsync(_currentUser!.Id, _customer.Id);

            if (_shoppingCart != null)
            {
                _showShoppingCart = true;
                _showOrderHistory = false;
            }
            else
            {
                _severity = Severity.Warning;
                _severityText = "No shopping cart found for the customer.";
            }
        }
        else
        {
            _severity = Severity.Warning;
            _severityText = "Customer not found.";
        }
    }

    private async Task LoadOrderHistory(string customerId)
    {
        _customerId = customerId;
        // Reset the order history for the new customer
        _orderHistory.Clear();
        _shoppingCart = null;
        await LoadCustomer();

        if (_customer != null)
        {
            _orderHistory = (await AccessCustomerDataUseCase.GetOrderHistoryAsync(_currentUser!.Id, _customer.Id)).ToList();

            if (_orderHistory.Any())
            {
                _showShoppingCart = false;
                _showOrderHistory = true;
            }
            else
            {
                _severity = Severity.Warning;
                _severityText = "No order history found for the customer.";
            }
        }
        else
        {
            _severity = Severity.Warning;
            _severityText = "Customer not found.";
        }
    }

    private async Task RemoveFromCart(Guid productId)
    {
        if (_shoppingCart == null || _customer == null)
        {
            return;
        }

        var input = new RemoveItemFromCartInput(_customer.Id, productId, 1);
        await RemoveItemFromCartUseCase.RemoveItemFromCartAsync(input);
        await LoadCart(_customer.Id.ToString());
    }
}