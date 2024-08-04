using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class OrderHistory
{
    [Inject]
    private IOrderRepository OrderRepository { get; set; } = null!;

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    private List<Order> _orderHistory = new();
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        var currentUser = await AuthenticationService.GetCurrentUserAsync();
        if (currentUser != null)
        {
            _orderHistory = (await OrderRepository.GetOrdersByUserIdAsync(currentUser.Id)).ToList();
        }

        _isLoading = false;
    }
}