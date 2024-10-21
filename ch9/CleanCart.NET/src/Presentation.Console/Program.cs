using Application.Interfaces.Data;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Presentation.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Explicitly set the environment to Development for demo purposes
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        // Build the host to initialize services via Startup
        var builder = CreateHostBuilder(args).Build();

        // Display a welcome message
        DisplayWelcomeMessage();

        // Prompt the user for a valid User ID and retrieve the shopping cart
        await RetrieveAndDisplayShoppingCartAsync(builder);

        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();

        await builder.RunAsync();
    }

    // Method to build the host
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                var startup = new Startup();
                startup.ConfigureServices(services);
            });

    // Display a friendly welcome message to introduce the app
    private static void DisplayWelcomeMessage()
    {
        System.Console.WriteLine("=============================================");
        System.Console.WriteLine("    Welcome to the Shopping Cart Console App! ");
        System.Console.WriteLine("=============================================");
        System.Console.WriteLine("In this demo, you can enter a User ID to view the items in the shopping cart.");
        System.Console.WriteLine();
    }

    // Method to handle user input and fetch shopping cart items
    private static async Task RetrieveAndDisplayShoppingCartAsync(IHost builder)
    {
        bool isValidUserId = false;
        while (!isValidUserId)
        {
            // Prompt the user for their User ID
            System.Console.WriteLine("Please enter a User ID (or press 'X' to exit):");

            var input = System.Console.ReadLine();
            if (input?.ToUpper() == "X")
            {
                return;
            }

            // Validate the User ID input
            if (Guid.TryParse(input, out var userId))
            {
                // Resolve the IShoppingCartRepository from DI
                var shoppingCartRepo = builder.Services.GetService<IShoppingCartRepository>();
                var shoppingCart = await shoppingCartRepo?.GetByUserIdAsync(userId);

                // Display the shopping cart items, or notify if empty
                DisplayShoppingCart(shoppingCart);

                isValidUserId = true;
            }
            else
            {
                System.Console.WriteLine("Invalid User ID format. Please try again.");
            }
        }
    }

    // Method to display shopping cart items in a formatted table
    private static void DisplayShoppingCart(ShoppingCart? shoppingCart)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Shopping Cart Items:");
        System.Console.WriteLine("--------------------------------------------------------");
        System.Console.WriteLine("{0,-40} {1,-20} {2,-10} {3,-10}", "ID", "Product Name", "Price", "Quantity");
        System.Console.WriteLine("--------------------------------------------------------");

        if (shoppingCart?.Items is not null && shoppingCart.Items.Any())
        {
            foreach (var item in shoppingCart.Items)
            {
                var truncatedProductName = TruncateString(item.ProductName, 20);
                System.Console.WriteLine("{0,-40} {1,-20} {2,-10:C} {3,-10}", item.ProductId, truncatedProductName, item.ProductPrice, item.Quantity);
            }
        }
        else
        {
            System.Console.WriteLine("Your shopping cart is empty.");
        }

        System.Console.WriteLine("--------------------------------------------------------");
    }

    // Helper method to truncate strings and add ellipsis if necessary
    private static string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }
}