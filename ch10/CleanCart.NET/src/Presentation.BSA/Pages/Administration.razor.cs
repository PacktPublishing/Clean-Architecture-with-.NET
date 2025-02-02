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

    private List<Product> _products = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        _products = await ProductRepository.GetAllAsync();
    }

    private void OnStockLevelChanged(Product product, int stockLevel)
    {
        product.UpdateStockLevel(stockLevel);
    }

    private async Task UpdateStockLevel(Guid productId, int stockLevel)
    {
        User user = await AuthenticationService.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User not found.");
        await ManageProductInventoryUseCase.UpdateProductInventoryAsync(user.Id, productId, stockLevel);

        var productName = _products.First(p => p.Id == productId).Name;
        Snackbar.Add($"{productName} stock updated.", Severity.Success);
    }
}