using Application.Interfaces.Auth;
using Application.Operations.UseCases.ManageProductInventory;
using Domain.Entities;
using EntityAxis.MediatR.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Presentation.BSA.Pages;

[Authorize(Policy = "Administrator")]
public partial class Administration
{
    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IMediator Mediator { get; set; } = null!;

    private List<Product> _products = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        var query = new GetAllEntitiesQuery<Product, Guid>();
        _products = await Mediator.Send(query);
    }

    private void OnStockLevelChanged(Product product, int stockLevel)
    {
        product.UpdateStockLevel(stockLevel);
    }

    private async Task UpdateStockLevel(Guid productId, int stockLevel)
    {
        User user = await AuthenticationService.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User not found.");
        var command = new UpdateProductInventoryCommand(user.Id, productId, stockLevel);
        await Mediator.Send(command);

        var productName = _products.First(p => p.Id == productId).Name;
        Snackbar.Add($"{productName} stock updated.", Severity.Success);
    }
}