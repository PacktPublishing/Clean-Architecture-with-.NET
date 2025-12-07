using Application.Interfaces.Auth;
using Application.Operations.UseCases.AccessCustomerData;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class OrderHistory
{
    [Inject]
    private IMediator Mediator { get; set; } = null!;

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
            var query = new AccessCustomerOrderHistoryQuery(currentUser.Id, currentUser.Id);
            _orderHistory = (await Mediator.Send(query)).ToList();
        }

        _isLoading = false;
    }
}