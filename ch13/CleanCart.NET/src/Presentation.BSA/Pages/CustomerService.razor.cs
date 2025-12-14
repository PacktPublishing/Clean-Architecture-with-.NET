using Application.Interfaces.Auth;
using Application.Operations.UseCases.AccessCustomerData;
using Application.Operations.UseCases.RemoveItemFromCart;
using Domain.Entities;
using EntityAxis.MediatR.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Presentation.BSA.Pages;

[Authorize(Policy = "CustomerService")]
public partial class CustomerService
{
    private Severity _severity = Severity.Info;
    private string _severityText = "Search for a customer's cart or order history.";
    private string _customerUserId = string.Empty;
    private bool _showShoppingCart;
    private bool _showOrderHistory;
    private User? _customer;
    private User? _currentUser;
    private ShoppingCart? _shoppingCart;
    private List<Order> _orderHistory = new();

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private IMediator Mediator { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _currentUser = await AuthenticationService.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User not found.");
    }

    private async Task LoadCustomer()
    {
        if (!Guid.TryParse(_customerUserId, out var customerUserIdGuid))
        {
            _shoppingCart = null;
            _customer = null;
        }

        var query = new GetEntityByIdQuery<User, Guid>(customerUserIdGuid);
        _customer = await Mediator.Send(query);
    }

    private async Task LoadCart(string customerUserId)
    {
        _customerUserId = customerUserId;
        // Reset the cart for the new customer
        _shoppingCart = null;
        await LoadCustomer();

        if (_customer != null)
        {
            var query = new AccessCustomerCartQuery(_currentUser!.Id, _customer.Id);
            _shoppingCart = await Mediator.Send(query);

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

    private async Task LoadOrderHistory(string customerUserId)
    {
        _customerUserId = customerUserId;
        // Reset the order history for the new customer
        _orderHistory.Clear();
        _shoppingCart = null;
        await LoadCustomer();

        if (_customer != null)
        {
            var query = new AccessCustomerOrderHistoryQuery(_currentUser!.Id, _customer.Id);
            _orderHistory = (await Mediator.Send(query)).ToList();

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

        var command = new RemoveItemFromCartCommand(_customer.Id, productId, 1);
        await Mediator.Send(command);
        await LoadCart(_customer.Id.ToString());
    }
}