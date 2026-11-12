# Domain entities in application responses

Published 2026-11-11.

| Chapters | What changed | Reader action |
| --- | --- | --- |
| 9–14; chapter 9, p. 267 | Clarifies the distinction between input view models and application response models. The sample code is unchanged. | When adapting the samples, use response models to expose the data a component needs without exposing domain mutation methods. |

Chapter 9 explains how input view models separate form concerns from domain entities, but does not fully address the other direction: what the Application layer returns to the UI. The samples use separate models selectively to limit the types and mappings introduced in the walkthrough. Returning domain entities still exposes their public business methods to presentation code, and this limitation deserves a clearer explanation.

## Avoid returning a mutable domain entity

A query that returns the domain cart also exposes its behavior:

```csharp
public record GetCartQuery(Guid UserId) : IRequest<ShoppingCart?>;
```

A Razor component receiving that cart can call its public methods directly:

```csharp
private ShoppingCart? _cart;

private void AddItem(Product product)
{
    _cart?.AddItem(product.Id, product.Name, product.Price, 1);
}

private void RemoveItem(Guid productId)
{
    _cart?.RemoveItem(productId, 1);
}
```

These calls change the in-memory cart without executing the application's add-item or remove-item use cases. They do not automatically persist changes, but modified data can affect later operations if passed back into the application. Calling `AddItem` directly, for example, skips the stock check performed by the add-item use case.

The samples' checkout flow illustrates why this matters. It passes the displayed cart's items into the payment use case, which uses those items to create an order while calculating the total from the persisted cart. Directly modifying the displayed cart could therefore produce order items that differ from the cart used to calculate the total.

## Return data and request changes through use cases

A cart display needs item identifiers, names, prices, and quantities. It does not need access to the domain cart's mutation methods. The Application layer can define a response containing only that data:

```csharp
public record CartItemResponse(
    Guid ProductId,
    string ProductName,
    decimal ProductPrice,
    int Quantity);

public record CartResponse(
    IReadOnlyList<CartItemResponse> Items);

public record GetCartQuery(Guid UserId) : IRequest<CartResponse?>;
```

The query handler maps the domain entity to a response:

```csharp
public class GetCartQueryHandler(
    IShoppingCartQueryRepository cartRepository)
    : IRequestHandler<GetCartQuery, CartResponse?>
{
    public async Task<CartResponse?> Handle(
        GetCartQuery query,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(query.UserId);

        return cart is null
            ? null
            : new CartResponse(
                cart.Items.Select(item => new CartItemResponse(
                    item.ProductId,
                    item.ProductName,
                    item.ProductPrice,
                    item.Quantity)).ToArray());
    }
}
```

The component displays the response and sends commands when the user requests a change:

```csharp
private CartResponse? _cart;

private async Task AddItem(Guid productId)
{
    await Mediator.Send(
        new AddItemToCartCommand(_user.Id, productId, 1));

    await ReloadCart();
}

private async Task RemoveItem(Guid productId)
{
    await Mediator.Send(
        new RemoveItemFromCartCommand(_user.Id, productId, 1));

    await ReloadCart();
}

private async Task ReloadCart()
{
    _cart = await Mediator.Send(new GetCartQuery(_user.Id));
}
```

`CartResponse` has no `AddItem` or `RemoveItem` methods and contains no domain entity references. Application use cases load the domain entities, enforce the relevant rules, invoke domain behavior, and persist changes. Reloading the response then updates the display.

Response models reduce accidental access to domain behavior. They do not replace application validation or authoritative data loading. For checkout, the application should obtain the items and prices used to create the order and calculate its total from the same authoritative cart, rather than trusting a previously displayed response.

## Applying this to the book samples

These examples illustrate a stronger response boundary, rather than changes already applied to the chapter projects. They use the MediatR conventions and cart commands from chapters 12–14. The same separation applies to the use-case interfaces in earlier chapters.

Compare the samples' [cart query](../../ch14/CleanCart.NET/src/Application/Operations/UseCases/AccessCustomerData/AccessCustomerShoppingCartQuery.cs), [add-item handler](../../ch14/CleanCart.NET/src/Application/Operations/UseCases/AddItemToCart/AddItemToCartCommandHandler.cs), and [remove-item handler](../../ch14/CleanCart.NET/src/Application/Operations/UseCases/RemoveItemFromCart/RemoveItemFromCartCommandHandler.cs). The intended mutation flows go through application use cases, but returning domain entities does not prevent components from bypassing that convention.

This concern is especially visible with rich domain entities that expose business operations. Anemic models resemble DTOs because they contain little or no behavior, but sharing mutable models still allows direct state changes and couples presentation code to their structure.

An input view model describes what a user can submit. An application response model describes what a caller needs to receive. Using one does not establish the boundary provided by the other.

This clarification follows [reader feedback in issue #7](https://github.com/PacktPublishing/Clean-Architecture-with-.NET/issues/7).
