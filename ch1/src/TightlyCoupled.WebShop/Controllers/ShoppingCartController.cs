using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using TightlyCoupled.WebShop.Models;
using TightlyCoupled.WebShop.Services;

namespace TightlyCoupled.WebShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        public List<Item> Items { get; set; } = new List<Item>();

        // GET: ShoppingCart
        public IActionResult Index()
        {
            // TODO: In the future, get the cart for the current authenticated user
            // For now, return the current cart items
            return View(Items);
        }

        // POST: ShoppingCart/AddItem
        [HttpPost]
        public IActionResult AddItem(string name, double price, int quantity = 1)
        {
            if (!string.IsNullOrEmpty(name) && price > 0 && quantity > 0)
            {
                // Check if item already exists in cart
                var existingItem = Items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    TempData["SuccessMessage"] = $"Updated {name} quantity in cart. New quantity: {existingItem.Quantity}";
                }
                else
                {
                    Items.Add(new Item { Name = name, Price = price, Quantity = quantity });
                    TempData["SuccessMessage"] = $"Added {name} to cart!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid item details. Please try again.";
            }
            
            return RedirectToAction("Index");
        }

        // POST: ShoppingCart/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var itemToRemove = Items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                {
                    Items.Remove(itemToRemove);
                    TempData["SuccessMessage"] = $"Removed {name} from cart.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Item not found in cart.";
                }
            }
            
            return RedirectToAction("Index");
        }

        public bool Checkout(string shippingOption, string customerAddress)
        {
            double totalPrice = 0;

            // Validate the items in the cart
            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    Console.WriteLine("Invalid item found in cart.");
                    return false;
                }
            }

            // Calculate total price based on item price and quantity
            foreach (var item in Items)
            {
                totalPrice += item.Price * item.Quantity;
            }

            // Call an internal HTTP service to get tax rate
            HttpClient httpClient = new HttpClient();
            double taxRate = 0;
            try
            {
                var response = httpClient.GetStringAsync($"http://taxservice.example.com/getTaxRate?address={customerAddress}").Result;
                taxRate = Convert.ToDouble(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not fetch tax rate: {ex.Message}");
                return false;
            }

            // Calculate shipping costs
            double shippingCost = 0;
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

            // Using Entity Framework with transaction support
            using (var dbContext = new DbContext("connectionStringHere"))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in Items)
                        {
                            // Simulate updating the stock
                            Console.WriteLine($"Reducing stock for item: {item.Name}");

                            // Simulate saving the order
                            dbContext.Set<Order>().Add(new Order { /* properties here */ });
                        }

                        dbContext.SaveChanges();

                        // Process payment
                        var paymentProcessor = new PaymentProcessor();
                        var paymentResult = paymentProcessor.ProcessPayment(totalPrice);

                        if (paymentResult == "Declined")
                        {
                            Console.WriteLine("Payment declined. Transaction rolled back.");
                            SendEmail("Order Failed", "Your payment was declined.");
                            transaction.Rollback();
                            return false;
                        }

                        Console.WriteLine("Payment processed successfully.");
                        SendEmail("Order Confirmation", "Your order has been placed.");
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        SendEmail("Order Failed", "An error occurred during the order process.");
                        transaction.Rollback();
                        return false;
                    }
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
