using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
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
    private IProductRepository ProductRepository { get; set; } = null!;

    [Inject]
    private IManageProductInventoryUseCase ManageProductInventoryUseCase { get; set; } = null!;

    private MudTable<Product>? _table;

    private async Task<TableData<Product>> LoadServerData(TableState state, CancellationToken token)
    {
        var page = state.Page + 1; // MudTable is 0-based
        var pageSize = state.PageSize;

        var result = await ProductRepository.GetPagedAsync(page, pageSize);

        return new TableData<Product>
        {
            Items = result.Items,
            TotalItems = result.TotalCount
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

        await ManageProductInventoryUseCase
            .UpdateProductInventoryAsync(user.Id, productId, stockLevel);

        Snackbar.Add($"Stock level for '{productName}' updated to {stockLevel}.", Severity.Success);

        if (_table is not null)
            await _table.ReloadServerData();
    }
}