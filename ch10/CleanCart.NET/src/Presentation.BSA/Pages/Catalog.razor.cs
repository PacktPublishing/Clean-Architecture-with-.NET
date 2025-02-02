﻿using Application.Interfaces.Auth;
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
    private ShoppingCartState ShoppingCartState { get; set; } = null!;

    [Inject]
    private IAddItemToCartUseCase AddItemToCartUseCase { get; set; } = null!;

    private User? _user;

    private List<Product> _products = new();
    private bool _isLoading = true;
    private List<Product> PagedProducts => _products.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 6;

    protected override async Task OnInitializedAsync()
    {
        _user ??= await AuthenticationService.GetCurrentUserAsync();
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        // We're loading all products, but in a real-world scenario, you'd want to implement pagination
        _products = await ProductRepository.GetAllAsync();
        _isLoading = false;
    }

    private void ChangePage(int page)
    {
        CurrentPage = page;
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
        ShoppingCartState.NotifyCartChanged();
    }
}