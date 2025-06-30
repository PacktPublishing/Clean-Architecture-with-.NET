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
using System.IO;
using System.Text;
using System.Xml;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace TightlyCoupled.WebShop.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShoppingCartController> _logger;
        
        // Hard-coded configuration - TERRIBLE practice for demonstrating tight coupling
        private const string LOG_FILE_PATH = @"C:\Logs\WebShop\cart_operations.log";
        private const string INVENTORY_FILE_PATH = @"C:\Data\inventory.xml";
        private const string CONFIG_FILE_PATH = @"C:\Config\webshop_config.ini";
        private const string TAX_SERVICE_URL = "http://taxservice.example.com";
        private const string ANALYTICS_URL = "http://analytics.company.com/api/events";
        private const string SMTP_SERVER = "smtp.company.com";
        private const int SMTP_PORT = 587;
        private const string EMAIL_USERNAME = "webshop@company.com";
        private const string EMAIL_PASSWORD = "hardcoded_password_123";
        private const string DB_CONNECTION = "Server=(localdb)\\mssqllocaldb;Database=TightlyCoupledWebShop;Trusted_Connection=true;MultipleActiveResultSets=true";

        public ShoppingCartController(ApplicationDbContext context, ILogger<ShoppingCartController> logger)
        {
            _context = context;
            _logger = logger;
            
            // Initialize file system dependencies in constructor - BAD!
            Directory.CreateDirectory(Path.GetDirectoryName(LOG_FILE_PATH) ?? @"C:\Logs\WebShop");
            Directory.CreateDirectory(Path.GetDirectoryName(INVENTORY_FILE_PATH) ?? @"C:\Data");
            Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE_PATH) ?? @"C:\Config");
            
            // Create initial config file if not exists
            if (!System.IO.File.Exists(CONFIG_FILE_PATH))
            {
                System.IO.File.WriteAllText(CONFIG_FILE_PATH, "max_cart_items=10\nmax_price_per_item=1000\nwarehouse_email=warehouse@company.com");
            }
        }
        
        // Tightly coupled file system logging - mixing concerns
        private void WriteToLogFile(string message)
        {
            try
            {
                System.IO.File.AppendAllText(LOG_FILE_PATH, $"{message}{Environment.NewLine}");
            }
            catch
            {
                // Swallow exceptions - another bad practice
            }
        }
        
        // Business rules hard-coded and mixed with infrastructure
        private bool ValidateBusinessRules(string itemName, int quantity, decimal price)
        {
            // Read business rules from config file - tight file coupling
            var configLines = System.IO.File.ReadAllLines(CONFIG_FILE_PATH);
            var maxItems = int.Parse(configLines[0].Split('=')[1]);
            var maxPrice = decimal.Parse(configLines[1].Split('=')[1]);
            
            if (quantity > maxItems)
            {
                WriteToLogFile($"[{DateTime.Now}] BUSINESS RULE VIOLATION: Quantity {quantity} exceeds max {maxItems}");
                return false;
            }
            
            if (price > maxPrice)
            {
                WriteToLogFile($"[{DateTime.Now}] BUSINESS RULE VIOLATION: Price {price:C} exceeds max {maxPrice:C}");
                return false;
            }
            
            // Special business rules for specific items - hard-coded
            if (itemName.ToLower().Contains("premium") && DateTime.Now.Hour < 9)
            {
                WriteToLogFile($"[{DateTime.Now}] BUSINESS RULE: Premium items not available before 9 AM");
                return false;
            }
            
            return true;
        }
        
        // Tightly coupled inventory management via XML files
        private void UpdateInventoryFile(string itemName, int quantityChange)
        {
            try
            {
                var doc = new XmlDocument();
                if (System.IO.File.Exists(INVENTORY_FILE_PATH))
                {
                    doc.Load(INVENTORY_FILE_PATH);
                }
                else
                {
                    doc.LoadXml("<inventory></inventory>");
                }
                
                var itemNode = doc.SelectSingleNode($"//item[@name='{itemName}']");
                if (itemNode?.Attributes != null)
                {
                    var currentStock = int.Parse(itemNode.Attributes["stock"]?.Value ?? "100");
                    itemNode.Attributes["stock"]!.Value = (currentStock - quantityChange).ToString();
                }
                else
                {
                    // Create new item with default stock
                    var newItem = doc.CreateElement("item");
                    newItem.SetAttribute("name", itemName);
                    newItem.SetAttribute("stock", (100 - quantityChange).ToString());
                    doc.DocumentElement!.AppendChild(newItem);
                }
                
                doc.Save(INVENTORY_FILE_PATH);
                WriteToLogFile($"[{DateTime.Now}] INVENTORY: Updated {itemName} stock");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now}] INVENTORY ERROR: {ex.Message}");
            }
        }
        
        // Hard-coded analytics integration - tightly coupled to external service
        private void SendAnalyticsData(string userEmail, string action, string itemName, int quantity, decimal price)
        {
            try
            {
                var analyticsData = new
                {
                    User = userEmail,
                    Action = action,
                    Item = itemName,
                    Quantity = quantity,
                    Price = price,
                    Timestamp = DateTime.UtcNow,
                    ServerName = Environment.MachineName,
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };
                
                var json = JsonSerializer.Serialize(analyticsData);
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(2); // Short timeout
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Synchronous call - blocks the request
                var response = httpClient.PostAsync(ANALYTICS_URL, content).Result;
                
                WriteToLogFile($"[{DateTime.Now}] ANALYTICS: Sent {action} data for {userEmail}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now}] ANALYTICS ERROR: {ex.Message}");
            }
        }

        // GET: ShoppingCart
        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.Identity?.Name ?? "Unknown";
                
                WriteToLogFile($"[{DateTime.Now}] User {userEmail} accessing cart");
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Anonymous user attempted to view shopping cart");
                    WriteToLogFile($"[{DateTime.Now}] SECURITY: Anonymous access attempt from IP: {Request.HttpContext.Connection.RemoteIpAddress}");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Direct SQL query instead of using EF properly - tight database coupling
                var cartItems = new List<CartItem>();
                
                using (var connection = new SqlConnection(DB_CONNECTION))
                {
                    connection.Open();
                    // SQL injection vulnerability for demonstration
                    var command = new SqlCommand($"SELECT * FROM CartItems WHERE UserId = '{userId}'", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cartItems.Add(new CartItem
                            {
                                Id = (int)reader["Id"],
                                UserId = reader["UserId"].ToString()!,
                                ItemName = reader["ItemName"].ToString()!,
                                Price = (decimal)reader["Price"],
                                Quantity = (int)reader["Quantity"],
                                DateAdded = (DateTime)reader["DateAdded"]
                            });
                        }
                    }
                }

                WriteToLogFile($"[{DateTime.Now}] User {userEmail} viewed cart with {cartItems.Count} items");
                SendAnalyticsData(userEmail, "ViewCart", "", cartItems.Count, cartItems.Sum(c => c.Price * c.Quantity));

                return View(cartItems);
            }
            catch (Exception ex)
            {
                var userEmail = User.Identity?.Name ?? "Unknown";
                WriteToLogFile($"[{DateTime.Now}] ERROR for {userEmail}: {ex.Message}");
                _logger.LogError(ex, "Error occurred while loading shopping cart for user {UserEmail}", userEmail);
                
                // Send error notification to warehouse - hard-coded email
                SendErrorNotificationToWarehouse(userEmail, ex.Message);
                throw;
            }
        }

        // POST: ShoppingCart/AddItem
        [HttpPost]
        public IActionResult AddItem(string name, decimal price, int quantity = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.Identity?.Name ?? "Unknown";
                
                if (string.IsNullOrEmpty(userId))
                {
                    WriteToLogFile($"[{DateTime.Now}] SECURITY: Anonymous user tried to add {name}");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                WriteToLogFile($"[{DateTime.Now}] User {userEmail} adding item {name}");

                // Validate business rules
                if (!ValidateBusinessRules(name, quantity, price))
                {
                    TempData["ErrorMessage"] = "Business rules validation failed.";
                    return RedirectToAction("Index");
                }
                
                if (!string.IsNullOrEmpty(name) && price > 0 && quantity > 0)
                {
                    // Direct SQL operations - bypassing EF
                    using (var connection = new SqlConnection(DB_CONNECTION))
                    {
                        connection.Open();
                        
                        // Check existing item with SQL injection vulnerability
                        var checkCommand = new SqlCommand($"SELECT * FROM CartItems WHERE UserId = '{userId}' AND LOWER(ItemName) = LOWER('{name}')", connection);
                        var existingId = 0;
                        var existingQuantity = 0;
                        
                        using (var reader = checkCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingId = (int)reader["Id"];
                                existingQuantity = (int)reader["Quantity"];
                            }
                        }
                        
                        if (existingId > 0)
                        {
                            // Update existing item
                            var newQuantity = existingQuantity + quantity;
                            var updateCommand = new SqlCommand($"UPDATE CartItems SET Quantity = {newQuantity} WHERE Id = {existingId}", connection);
                            updateCommand.ExecuteNonQuery();
                            
                            WriteToLogFile($"[{DateTime.Now}] Updated {name} for {userEmail}: {existingQuantity} -> {newQuantity}");
                            TempData["SuccessMessage"] = $"Updated {name} quantity in cart. New quantity: {newQuantity}";
                        }
                        else
                        {
                            // Insert new item
                            var insertCommand = new SqlCommand($"INSERT INTO CartItems (UserId, ItemName, Price, Quantity, DateAdded) VALUES ('{userId}', '{name}', {price}, {quantity}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}')", connection);
                            insertCommand.ExecuteNonQuery();
                            
                            WriteToLogFile($"[{DateTime.Now}] Added new {name} for {userEmail}");
                            TempData["SuccessMessage"] = $"Added {name} to cart!";
                        }
                    }
                    
                    // Update file-based inventory
                    UpdateInventoryFile(name, quantity);
                    
                    // Send analytics
                    SendAnalyticsData(userEmail, "AddToCart", name, quantity, price);
                    
                    // Trigger warehouse notification for high-value items
                    if (price * quantity > 500)
                    {
                        NotifyWarehouseHighValueItem(userEmail, name, quantity, price);
                    }
                }
                else
                {
                    WriteToLogFile($"[{DateTime.Now}] VALIDATION ERROR for {userEmail}: Invalid item details");
                    TempData["ErrorMessage"] = "Invalid item details. Please try again.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var userEmail = User.Identity?.Name ?? "Unknown";
                WriteToLogFile($"[{DateTime.Now}] EXCEPTION for {userEmail}: {ex.Message}");
                _logger.LogError(ex, "Error occurred while adding item {ItemName} to cart for user {UserEmail}", name, userEmail);
                throw;
            }
        }

        // Hard-coded warehouse notification
        private void NotifyWarehouseHighValueItem(string userEmail, string itemName, int quantity, decimal price)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("WebShop System", EMAIL_USERNAME));
                message.To.Add(new MailboxAddress("Warehouse", "warehouse@company.com"));
                message.Subject = "High Value Item Added to Cart";
                message.Body = new TextPart("plain") 
                { 
                    Text = $"User {userEmail} added high-value item:\n\nItem: {itemName}\nQuantity: {quantity}\nPrice: {price:C}\nTotal: {quantity * price:C}\n\nTime: {DateTime.Now}"
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(SMTP_SERVER, SMTP_PORT, false);
                    smtpClient.Authenticate(EMAIL_USERNAME, EMAIL_PASSWORD);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
                
                WriteToLogFile($"[{DateTime.Now}] WAREHOUSE: Notified about high-value item for {userEmail}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now}] WAREHOUSE EMAIL ERROR: {ex.Message}");
            }
        }
        
        // Error notification method - tightly coupled to specific email infrastructure
        private void SendErrorNotificationToWarehouse(string userEmail, string errorMessage)
        {
            try
            {
                var configLines = System.IO.File.ReadAllLines(CONFIG_FILE_PATH);
                var warehouseEmail = configLines[2].Split('=')[1];
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("WebShop Error System", EMAIL_USERNAME));
                message.To.Add(new MailboxAddress("Warehouse", warehouseEmail));
                message.Subject = "WebShop Error Alert";
                message.Body = new TextPart("plain") 
                { 
                    Text = $"Error occurred for user {userEmail}:\n\n{errorMessage}\n\nTime: {DateTime.Now}\nServer: {Environment.MachineName}"
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(SMTP_SERVER, SMTP_PORT, false);
                    smtpClient.Authenticate(EMAIL_USERNAME, EMAIL_PASSWORD);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
            }
            catch
            {
                // Swallow email errors
            }
        }

        // Simplified methods for the rest - keeping the original functionality
        [HttpPost]
        public async Task<IActionResult> RemoveItem(string name)
        {
            // Original implementation but with added file logging
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            WriteToLogFile($"[{DateTime.Now}] User {userEmail} removing item {name}");
            
            var itemToRemove = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemName.ToLower() == name.ToLower());
            
            if (itemToRemove != null)
            {
                _context.CartItems.Remove(itemToRemove);
                await _context.SaveChangesAsync();
                SendAnalyticsData(userEmail, "RemoveFromCart", name, itemToRemove.Quantity, itemToRemove.Price);
                TempData["SuccessMessage"] = $"Removed {name} from cart.";
            }
            
            return RedirectToAction("Index");
        }

        public async Task<bool> Checkout(string shippingOption, string customerAddress)
        {
            // Keep original checkout logic but add more tight coupling
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name ?? "Unknown";
            
            WriteToLogFile($"[{DateTime.Now}] User {userEmail} starting checkout");
            
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            
            if (!cartItems.Any()) return false;

            decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);

            // Hard-coded tax service call
            HttpClient httpClient = new HttpClient();
            decimal taxRate = 0;
            try
            {
                var response = httpClient.GetStringAsync($"{TAX_SERVICE_URL}/getTaxRate?address={customerAddress}").Result;
                taxRate = Convert.ToDecimal(response);
            }
            catch
            {
                taxRate = 0.08m; // Default tax rate
            }

            decimal shippingCost = shippingOption == "Express" ? 25 : 10;
            totalPrice += (totalPrice * taxRate) + shippingCost;

            // Process payment with hard-coded service
            var paymentProcessor = new PaymentProcessor();
            var paymentResult = paymentProcessor.ProcessPayment(totalPrice);

            if (paymentResult == "Declined")
            {
                SendEmail("Order Failed", "Your payment was declined.");
                WriteToLogFile($"[{DateTime.Now}] PAYMENT DECLINED for {userEmail}");
                return false;
            }

            // Create order using EF
            var order = new Order 
            { 
                UserId = userId,
                CustomerAddress = customerAddress,
                ShippingOption = shippingOption,
                TotalAmount = totalPrice,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            SendEmail("Order Confirmation", "Your order has been placed.");
            SendAnalyticsData(userEmail, "Checkout", "", cartItems.Count, totalPrice);
            WriteToLogFile($"[{DateTime.Now}] CHECKOUT COMPLETE for {userEmail}: Order {order.Id}");

            return true;
        }

        private void SendEmail(string subject, string body)
        {
            // Keep original email implementation
            var userEmail = User.Identity?.Name ?? "user@example.com";
            
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("No Reply", "no-reply@example.com"));
                message.To.Add(new MailboxAddress("User", userEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(SMTP_SERVER, SMTP_PORT, false);
                    smtpClient.Authenticate(EMAIL_USERNAME, EMAIL_PASSWORD);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
                
                WriteToLogFile($"[{DateTime.Now}] EMAIL: Sent {subject} to {userEmail}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now}] EMAIL ERROR: {ex.Message}");
            }
        }
    }
}
