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
using TightlyCoupled.WebShop.Data;
using TightlyCoupled.WebShop.Models;
using TightlyCoupled.WebShop.Services;

namespace TightlyCoupled.WebShop.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ShoppingCart
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: ShoppingCart/AddItem
        [HttpPost]
        public async Task<IActionResult> AddItem(string name, decimal price, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (!string.IsNullOrEmpty(name) && price > 0 && quantity > 0)
            {
                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemName.ToLower() == name.ToLower());
                
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
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
                    TempData["SuccessMessage"] = $"Added {name} to cart!";
                }
                
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid item details. Please try again.";
            }
            
            return RedirectToAction("Index");
        }

        // POST: ShoppingCart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (!string.IsNullOrEmpty(name))
            {
                var itemToRemove = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemName.ToLower() == name.ToLower());
                
                if (itemToRemove != null)
                {
                    _context.CartItems.Remove(itemToRemove);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Removed {name} from cart.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Item not found in cart.";
                }
            }
            
            return RedirectToAction("Index");
        }

        public async Task<bool> Checkout(string shippingOption, string customerAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return false;
            }

            decimal totalPrice = 0;

            // Validate the items in the cart
            foreach (var item in cartItems)
            {
                if (string.IsNullOrEmpty(item.ItemName))
                {
                    Console.WriteLine("Invalid item found in cart.");
                    return false;
                }
            }

            // Calculate total price based on item price and quantity
            foreach (var item in cartItems)
            {
                totalPrice += item.Price * item.Quantity;
            }

            // Call an internal HTTP service to get tax rate
            HttpClient httpClient = new HttpClient();
            decimal taxRate = 0;
            try
            {
                var response = httpClient.GetStringAsync($"http://taxservice.example.com/getTaxRate?address={customerAddress}").Result;
                taxRate = Convert.ToDecimal(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch tax rate: {ex.Message}");
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

            // Add tax and shipping to the total price
            totalPrice += (totalPrice * taxRate) + shippingCost;

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

                    // Add order items
                    foreach (var cartItem in cartItems)
                    {
                        // Simulate updating the stock
                        Console.WriteLine($"Reducing stock for item: {cartItem.ItemName}");

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

                    // Process payment
                    var paymentProcessor = new PaymentProcessor();
                    var paymentResult = paymentProcessor.ProcessPayment(totalPrice);

                    if (paymentResult == "Declined")
                    {
                        Console.WriteLine("Payment declined. Transaction rolled back.");
                        SendEmail("Order Failed", "Your payment was declined.");
                        await transaction.RollbackAsync();
                        return false;
                    }

                    Console.WriteLine("Payment processed successfully.");
                    SendEmail("Order Confirmation", "Your order has been placed.");
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    SendEmail("Order Failed", "An error occurred during the order process.");
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            return true;
        }

        private void SendEmail(string subject, string body)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email could not be sent: {ex.Message}");
            }
        }
    }
}
