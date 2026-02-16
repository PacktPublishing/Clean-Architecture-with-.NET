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

    private MudTable<Product>? _table;

    private async Task<TableData<Product>> LoadServerData(TableState state, CancellationToken token)
    {
        var page = state.Page + 1; // MudTable is 0-based
        var pageSize = state.PageSize;

        var query = new GetPagedEntitiesQuery<Product, Guid>(page, pageSize);
        var result = await Mediator.Send(query, token);

        return new TableData<Product>
        {
            Items = result.Items,
            TotalItems = result.TotalItemCount
        };
    }

    private void OnStockLevelChanged(Product product, int stockLevel)
    {
        product.UpdateStockLevel(stockLevel);
    }

    private async Task UpdateStockLevel(Guid productId, int stockLevel, string productName)
    {
        var user = await AuthenticationService.GetCurrentUserAsync()
                   ?? throw new UnauthorizedAccessException("User not found.");

        var command = new UpdateProductInventoryCommand(user.Id, productId, stockLevel);
        await Mediator.Send(command);

        Snackbar.Add($"Stock level for '{productName}' updated to {stockLevel}.", Severity.Success);

        if (_table is not null)
            await _table.ReloadServerData();
    }
}