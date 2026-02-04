using Application.Interfaces.Data;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Console;
using Presentation.Common.Extensions;

// Explicitly set environment (demo-friendly, optional)
const string environmentName = "Development";
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentName);

// Create the application builder (minimal hosting)
var builder = Host.CreateApplicationBuilder(args);

// Configure application settings
string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
string path = Directory.GetParent(assemblyLocation)!.FullName;
builder.Configuration.SetBasePath(path);
builder.Configuration.AddCoreLayerConfiguration();
builder.Configuration.AddAzureKeyVault(environmentName);

// Register services using your existing composition root
var serviceComposition = new PresentationServiceComposition(builder.Configuration);
serviceComposition.ConfigureServices(builder.Services);

// Build the host
using var host = builder.Build();

// Run the application logic
DisplayWelcomeMessage();
await RetrieveAndDisplayShoppingCartAsync(host);

// Graceful shutdown
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
await host.StopAsync();


// ------------------------------------------------------------
// Application logic (unchanged, now top-level helpers)
// ------------------------------------------------------------

static void DisplayWelcomeMessage()
{
    Console.WriteLine("=============================================");
    Console.WriteLine("    Welcome to the Shopping Cart Console App! ");
    Console.WriteLine("=============================================");
    Console.WriteLine("In this demo, you can enter a User ID to view the items in the shopping cart.");
    Console.WriteLine();
}

static async Task RetrieveAndDisplayShoppingCartAsync(IHost host)
{
    while (true)
    {
        Console.WriteLine("Please enter a User ID (or press 'X' to exit):");
        var input = Console.ReadLine();

        if (input?.Equals("X", StringComparison.OrdinalIgnoreCase) == true)
            return;

        if (!Guid.TryParse(input, out var userId))
        {
            Console.WriteLine("Invalid User ID format. Please try again.");
            continue;
        }

        var shoppingCartRepo = host.Services.GetRequiredService<IShoppingCartQueryRepository>();
        var shoppingCart = await shoppingCartRepo.GetByUserIdAsync(userId);

        DisplayShoppingCart(shoppingCart);
        return;
    }
}

static void DisplayShoppingCart(ShoppingCart? shoppingCart)
{
    Console.WriteLine();
    Console.WriteLine("Shopping Cart Items:");
    Console.WriteLine("--------------------------------------------------------");
    Console.WriteLine("{0,-40} {1,-20} {2,-10} {3,-10}", "ID", "Product Name", "Price", "Quantity");
    Console.WriteLine("--------------------------------------------------------");

    if (shoppingCart?.Items is { Count: > 0 })
    {
        foreach (var item in shoppingCart.Items)
        {
            Console.WriteLine(
                "{0,-40} {1,-20} {2,-10:C} {3,-10}",
                item.ProductId,
                TruncateString(item.ProductName, 20),
                item.ProductPrice,
                item.Quantity
            );
        }
    }
    else
    {
        Console.WriteLine("Your shopping cart is empty.");
    }

    Console.WriteLine("--------------------------------------------------------");
}

static string TruncateString(string value, int maxLength)
{
    if (string.IsNullOrEmpty(value)) return value;
    return value.Length <= maxLength
        ? value
        : value[..(maxLength - 3)] + "...";
}
