using Application.Interfaces.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class OrderConfirmation
{
    [Parameter]
    public Guid OrderId { get; set; }

    [Inject]
    private IOrderRepository OrderRepository { get; set; } = default!;

    private Order? _order;

    protected override async Task OnInitializedAsync()
    {
        _order = await OrderRepository.GetOrderByIdAsync(OrderId);
    }
}