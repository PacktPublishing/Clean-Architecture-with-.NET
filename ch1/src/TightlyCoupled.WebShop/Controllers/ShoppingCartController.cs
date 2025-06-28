using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TightlyCoupled.WebShop.Data;
using TightlyCoupled.WebShop.Models;
using TightlyCoupled.WebShop.Services;

namespace TightlyCoupled.WebShop.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ApplicationDbContext context, ILogger<ShoppingCartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ShoppingCart
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Anonymous user attempted to view shopping cart");
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _logger.LogInformation("User {UserEmail} viewed their shopping cart containing {ItemCount} items", userEmail, cartItems.Count);

            return View(cartItems);
        }

        // POST: ShoppingCart/AddItem
        [HttpPost]
        public async Task<IActionResult> AddItem(string name, decimal price, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Anonymous user attempted to add item {ItemName} to cart", name);
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            _logger.LogInformation("User {UserEmail} (ID: {UserId}) attempting to add item {ItemName} (Price: {Price:C}, Quantity: {Quantity}) to cart", 
                userEmail, userId, name, price, quantity);

            if (!string.IsNullOrEmpty(name) && price > 0 && quantity > 0)
            {
                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemName.ToLower() == name.ToLower());
                
                if (existingItem != null)
                {
                    var oldQuantity = existingItem.Quantity;
                    existingItem.Quantity += quantity;
                    _logger.LogInformation("Updated existing cart item {ItemName} for user {UserEmail}: quantity changed from {OldQuantity} to {NewQuantity}", 
                        name, userEmail, oldQuantity, existingItem.Quantity);
                    TempData["SuccessMessage"] = $"Updated {name} quantity in cart. New quantity: {existingItem.Quantity}";
                }
                else
                {
                    var cartItem = new CartItem 
                    { 
                        UserId = userId,
                        ItemName = name, 
                        Price = price, 
                        Quantity = quantity 
                    };
                    _context.CartItems.Add(cartItem);
                    _logger.LogInformation("Added new cart item {ItemName} for user {UserEmail}: Price: {Price:C}, Quantity: {Quantity}", 
                        name, userEmail, price, quantity);
                    TempData["SuccessMessage"] = $"Added {name} to cart!";
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cart changes saved to database for user {UserEmail}", userEmail);
            }
            else
            {
                _logger.LogWarning("Invalid item details provided by user {UserEmail}: Name='{ItemName}', Price={Price}, Quantity={Quantity}", 
                    userEmail, name, price, quantity);
                TempData["ErrorMessage"] = "Invalid item details. Please try again.";
            }
            
            return RedirectToAction("Index");
        }

        // POST: ShoppingCart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Anonymous user attempted to remove item {ItemName} from cart", name);
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            _logger.LogInformation("User {UserEmail} (ID: {UserId}) attempting to remove item {ItemName} from cart", 
                userEmail, userId, name);

            if (!string.IsNullOrEmpty(name))
            {
                var itemToRemove = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemName.ToLower() == name.ToLower());
                
                if (itemToRemove != null)
                {
                    _logger.LogInformation("Removing cart item {ItemName} (Price: {Price:C}, Quantity: {Quantity}) for user {UserEmail}", 
                        itemToRemove.ItemName, itemToRemove.Price, itemToRemove.Quantity, userEmail);
                    
                    _context.CartItems.Remove(itemToRemove);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Successfully removed cart item {ItemName} for user {UserEmail}", name, userEmail);
                    TempData["SuccessMessage"] = $"Removed {name} from cart.";
                }
                else
                {
                    _logger.LogWarning("User {UserEmail} attempted to remove non-existent item {ItemName} from cart", userEmail, name);
                    TempData["ErrorMessage"] = "Item not found in cart.";
                }
            }
            else
            {
                _logger.LogWarning("User {UserEmail} attempted to remove item with empty name from cart", userEmail);
            }
            
            return RedirectToAction("Index");
        }

        public async Task<bool> Checkout(string shippingOption, string customerAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Anonymous user attempted to checkout");
                return false;
            }

            _logger.LogInformation("User {UserEmail} (ID: {UserId}) starting checkout process with shipping option: {ShippingOption}, address: {Address}", 
                userEmail, userId, shippingOption, customerAddress);

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                _logger.LogWarning("User {UserEmail} attempted to checkout with empty cart", userEmail);
                return false;
            }

            _logger.LogInformation("User {UserEmail} has {ItemCount} items in cart for checkout", userEmail, cartItems.Count);

            decimal totalPrice = 0;

            // Validate the items in the cart
            foreach (var item in cartItems)
            {
                if (string.IsNullOrEmpty(item.ItemName))
                {
                    _logger.LogError("Invalid cart item found for user {UserEmail}: empty item name", userEmail);
                    return false;
                }
            }

            // Calculate total price based on item price and quantity
            foreach (var item in cartItems)
            {
                totalPrice += item.Price * item.Quantity;
            }

            _logger.LogInformation("User {UserEmail} cart subtotal: {Subtotal:C}", userEmail, totalPrice);

            // Call an internal HTTP service to get tax rate
            HttpClient httpClient = new HttpClient();
            decimal taxRate = 0;
            try
            {
                _logger.LogInformation("Fetching tax rate for user {UserEmail} at address: {Address}", userEmail, customerAddress);
                var response = httpClient.GetStringAsync($"http://taxservice.example.com/getTaxRate?address={customerAddress}").Result;
                taxRate = Convert.ToDecimal(response);
                _logger.LogInformation("Tax rate retrieved for user {UserEmail}: {TaxRate:P}", userEmail, taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch tax rate for user {UserEmail} at address {Address}", userEmail, customerAddress);
                return false;
            }

            // Calculate shipping costs
            decimal shippingCost = 0;
            if (shippingOption == "Express")
            {
                shippingCost = 25;
            }
            else
            {
                shippingCost = 10;
            }

            _logger.LogInformation("Shipping cost calculated for user {UserEmail}: {ShippingCost:C} ({ShippingOption})", 
                userEmail, shippingCost, shippingOption);

            // Add tax and shipping to the total price
            var taxAmount = totalPrice * taxRate;
            totalPrice += taxAmount + shippingCost;
            
            _logger.LogInformation("Final order total for user {UserEmail}: {Total:C} (Subtotal: {Subtotal:C}, Tax: {Tax:C}, Shipping: {Shipping:C})", 
                userEmail, totalPrice, totalPrice - taxAmount - shippingCost, taxAmount, shippingCost);

            // Using Entity Framework Core with transaction support
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create order with order items
                    var order = new Order 
                    { 
                        UserId = userId,
                        CustomerAddress = customerAddress,
                        ShippingOption = shippingOption,
                        TotalAmount = totalPrice,
                        CreatedDate = DateTime.UtcNow
                    };
                    
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get order ID
                    
                    _logger.LogInformation("Order {OrderId} created for user {UserEmail} with {ItemCount} items", 
                        order.Id, userEmail, cartItems.Count);

                    // Add order items
                    foreach (var cartItem in cartItems)
                    {
                        // Simulate updating the stock
                        _logger.LogInformation("Reducing stock for item: {ItemName} (Quantity: {Quantity}) for order {OrderId}", 
                            cartItem.ItemName, cartItem.Quantity, order.Id);

                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ItemName = cartItem.ItemName,
                            Price = cartItem.Price,
                            Quantity = cartItem.Quantity
                        };
                        _context.OrderItems.Add(orderItem);
                    }

                    // Clear the cart
                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Cart cleared for user {UserEmail} after creating order {OrderId}", userEmail, order.Id);

                    // Process payment
                    var paymentProcessor = new PaymentProcessor();
                    var paymentResult = paymentProcessor.ProcessPayment(totalPrice);

                    if (paymentResult == "Declined")
                    {
                        _logger.LogWarning("Payment declined for user {UserEmail}, order {OrderId}. Transaction rolled back.", userEmail, order.Id);
                        SendEmail("Order Failed", "Your payment was declined.");
                        await transaction.RollbackAsync();
                        return false;
                    }

                    _logger.LogInformation("Payment processed successfully for user {UserEmail}, order {OrderId}", userEmail, order.Id);
                    SendEmail("Order Confirmation", "Your order has been placed.");
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Order {OrderId} completed successfully for user {UserEmail}. Total amount: {TotalAmount:C}", 
                        order.Id, userEmail, totalPrice);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during checkout for user {UserEmail}. Transaction rolled back.", userEmail);
                    SendEmail("Order Failed", "An error occurred during the order process.");
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            return true;
        }

        private void SendEmail(string subject, string body)
        {
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            try
            {
                _logger.LogInformation("Attempting to send email to user {UserEmail} with subject: {EmailSubject}", userEmail, subject);
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("No Reply", "no-reply@example.com"));
                message.To.Add(new MailboxAddress("User", "user@example.com"));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.example.com", 587, false);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
                
                _logger.LogInformation("Email sent successfully to user {UserEmail} with subject: {EmailSubject}", userEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to user {UserEmail} with subject: {EmailSubject}", userEmail, subject);
            }
        }
    }
}
