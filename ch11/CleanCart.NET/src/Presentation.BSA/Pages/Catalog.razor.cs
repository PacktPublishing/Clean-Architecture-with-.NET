using Application.Common.Models;
using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Application.UseCases.AddItemToCart;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Presentation.BSA.Services;

namespace Presentation.BSA.Pages;

[Authorize]
public partial class Catalog
{
    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private IProductRepository ProductRepository { get; set; } = null!;

    [Inject]
    private IUserRepository UserRepository { get; set; } = null!;

    [Inject]
    private ShoppingCartStateContainer ShoppingCartStateContainer { get; set; } = null!;

    [Inject]
    private IAddItemToCartUseCase AddItemToCartUseCase { get; set; } = null!;

    private User? _user;

    private PagedResult<Product>? _pagedResult;
    private bool _isLoading = true;

    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 6;

    protected override async Task OnInitializedAsync()
    {
        _user ??= await AuthenticationService.GetCurrentUserAsync();
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        _isLoading = true;

        _pagedResult = await ProductRepository.GetPagedAsync(CurrentPage, PageSize);

        _isLoading = false;
    }

    private async Task ChangePage(int page)
    {
        CurrentPage = page;
        await LoadProducts();
    }

    private async Task AddToCart(Guid productId)
    {
        if (_user == null)
        {
            return;
        }
        var input = new AddItemToCartInput(_user.Id, productId, 1);
        await AddItemToCartUseCase.AddItemToCartAsync(input);
        // Notify other components about the cart change
        ShoppingCartStateContainer.NotifyCartChanged();
    }
}