using Domain.Entities;
using EntityAxis.MediatR.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class OrderConfirmation
{
    [Parameter]
    public Guid OrderId { get; set; }

    [Inject]
    private IMediator Mediator { get; set; } = default!;

    private Order? _order;

    protected override async Task OnInitializedAsync()
    {
        var query = new GetEntityByIdQuery<Order, Guid>(OrderId);
        _order = await Mediator.Send(query);
    }
}