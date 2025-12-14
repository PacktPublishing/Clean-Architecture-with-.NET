using Application.Interfaces.Auth;
using Application.Operations.UseCases.AccessCustomerData;
using Bunit;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using MudBlazor;
using MudBlazor.Services;
using Presentation.BSA.Components;
using Presentation.BSA.Services;

namespace Presentation.UnitTests.ComponentTests;

public class ShoppingCartIconTests
{
    private readonly IAuthenticationService _authMock = Substitute.For<IAuthenticationService>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private readonly ShoppingCartState _cartState = new();

    private void ConfigureServices(BunitContext ctx)
    {
        ctx.Services.AddScoped(_ => _authMock);
        ctx.Services.AddScoped(_ => _mediator);
        ctx.Services.AddScoped(_ => _cartState);

        ctx.Services.AddMudServices();
        ctx.Services.AddMudPopoverService();
        ctx.Services.AddMudBlazorDialog();
        ctx.Services.AddMudBlazorSnackbar();
    }

    private static void ConfigureMudJsInterop(BunitJSInterop js)
    {
        js.Setup<int>("mudpopoverHelper.countProviders", _ => true);
        js.SetupVoid("mudPopover.initialize", _ => true).SetVoidResult();
        js.SetupVoid("mudPopover.connect", _ => true).SetVoidResult();
        js.SetupVoid("mudPopover.update", _ => true).SetVoidResult();
        js.SetupVoid("mudPopover.dispose", _ => true).SetVoidResult();
    }

    [Fact]
    public async Task ShoppingCartIcon_WhenCartIsEmpty_ShouldRenderEmptyCartIcon()
    {
        await using var ctx = new BunitContext();
        ConfigureMudJsInterop(ctx.JSInterop);
        ConfigureServices(ctx);

        var user = new User("username@example.com", "username@example.com", "Test User", new List<UserRole>());
        _authMock.GetCurrentUserAsync().Returns(user);

        var cart = new ShoppingCart(user.Id);
        _mediator.Send(Arg.Any<AccessCustomerShoppingCartQuery>(), Arg.Any<CancellationToken>()).Returns(cart);

        var cut = ctx.Render<Wrapper>(p => p.AddChildContent<ShoppingCartIcon>());

        // Wait for UI to update after OnInitializedAsync and cart load
        await cut.WaitForStateAsync(() => cut.FindComponent<MudBadge>().Instance.Content?.ToString() == "0");

        var badge = cut.FindComponent<MudBadge>();

        Assert.Equal("0", badge.Instance.Content?.ToString());
    }

    [Fact]
    public async Task ShoppingCartIcon_WhenCartHasItems_ShouldRenderBadgeWithItemCount()
    {
        await using var ctx = new BunitContext();
        ConfigureMudJsInterop(ctx.JSInterop);
        ConfigureServices(ctx);

        var user = new User("username@example.com", "username@example.com", "Test User", new List<UserRole>());
        _authMock.GetCurrentUserAsync().Returns(user);

        var p1 = new Product("Product 1", 10.00m, 10, "img.png");
        var p2 = new Product("Product 2", 20.00m, 10, "img.png");

        var cart = new ShoppingCart(user.Id);
        cart.AddItem(p1.Id, p1.Name, p1.Price, 5);
        cart.AddItem(p2.Id, p2.Name, p2.Price, 5);

        _mediator.Send(Arg.Any<AccessCustomerShoppingCartQuery>(), Arg.Any<CancellationToken>()).Returns(cart);

        var cut = ctx.Render<Wrapper>(p => p.AddChildContent<ShoppingCartIcon>());

        await cut.WaitForStateAsync(() => cut.FindComponent<MudBadge>().Instance.Content?.ToString() == cart.Items.Count.ToString());

        var badge = cut.FindComponent<MudBadge>();

        Assert.Equal(cart.Items.Count.ToString(), badge.Instance.Content?.ToString());
    }
}
