using Application.Interfaces.Auth;
using Application.Operations.UseCases.AddItemToCart;
using Domain.Entities;
using EntityAxis.Abstractions;
using EntityAxis.MediatR.Queries;
using MediatR;
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
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private ShoppingCartStateContainer ShoppingCartStateContainer { get; set; } = null!;

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

        var query = new GetPagedEntitiesQuery<Product, Guid>(CurrentPage, PageSize);
        _pagedResult = await Mediator.Send(query);

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
        var command = new AddItemToCartCommand(_user.Id, productId, 1);
        await Mediator.Send(command);
        // Notify other components about the cart change
        ShoppingCartStateContainer.NotifyCartChanged();
    }
}