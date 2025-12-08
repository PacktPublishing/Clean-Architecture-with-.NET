using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Xml;
using TightlyCoupled.WebShop.Data;
using TightlyCoupled.WebShop.Models;
using TightlyCoupled.WebShop.Services;
using TightlyCoupled.WebShop.ViewModels;

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

        public ShoppingCartController(ApplicationDbContext context,
                                        ILogger<ShoppingCartController> logger)
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
        
        // Manager approval email - another hard-coded coupling
        private void SendManagerApprovalEmail(string userEmail, decimal orderAmount, string approvalFilePath)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("WebShop Approval System", EMAIL_USERNAME));
                message.To.Add(new MailboxAddress("Manager", "manager@company.com")); // Hard-coded manager email
                message.Subject = $"Order Approval Required - {orderAmount:C}";
                message.Body = new TextPart("plain") 
                { 
                    Text = $@"
Manager Approval Required

Customer: {userEmail}
Order Amount: {orderAmount:C}
Request Time: {DateTime.Now}
Approval File: {approvalFilePath}

Please review and approve/reject this order.

To approve: Reply with 'APPROVE {Path.GetFileNameWithoutExtension(approvalFilePath)}'
To reject: Reply with 'REJECT {Path.GetFileNameWithoutExtension(approvalFilePath)}'

Best regards,
WebShop System"
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(SMTP_SERVER, SMTP_PORT, false);
                    smtpClient.Authenticate(EMAIL_USERNAME, EMAIL_PASSWORD);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
                
                WriteToLogFile($"[{DateTime.Now}] APPROVAL EMAIL: Sent to manager for {userEmail}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now}] APPROVAL EMAIL ERROR: {ex.Message}");
            }
        }

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

        public record CartDetails(string shippingOption, string customerAddress);

        [HttpPost]
        public IActionResult Checkout([FromBody]CartDetails details)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.Identity?.Name ?? "Unknown";
                
                // Update global state - bad practice from UserStatsViewComponent
                GlobalUtilities.CurrentUser = userEmail;
                GlobalUtilities.LastActivity = DateTime.Now;
                
                WriteToLogFile($"[{DateTime.Now}] CHECKOUT STARTED: User {userEmail} with shipping {details.shippingOption} to {details.customerAddress}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    WriteToLogFile($"[{DateTime.Now}] SECURITY: Anonymous checkout attempt");
                    return Unauthorized();
                }

                // Business rule validation using file-based config - tight coupling
                var configLines = System.IO.File.ReadAllLines(CONFIG_FILE_PATH);
                var maxOrderValue = decimal.Parse(configLines.FirstOrDefault(l => l.StartsWith("max_order_value="))?.Split('=')[1] ?? "5000");
                var requiresManagerApproval = bool.Parse(configLines.FirstOrDefault(l => l.StartsWith("manager_approval_required="))?.Split('=')[1] ?? "false");
                
                // Direct SQL query instead of using EF - SQL injection vulnerability
                var cartItems = new List<CartItem>();
                decimal subtotal = 0;
                
                using (var connection = new SqlConnection(DB_CONNECTION))
                {
                    connection.Open();
                    // Terrible SQL with injection vulnerability
                    var sql = $@"
                        SELECT ci.Id, ci.UserId, ci.ItemName, ci.Price, ci.Quantity, ci.DateAdded,
                               CASE WHEN ci.ItemName LIKE '%premium%' THEN ci.Price * 1.2 ELSE ci.Price END as AdjustedPrice
                        FROM CartItems ci
                        INNER JOIN AspNetUsers u ON ci.UserId = u.Id  
                        WHERE u.Email = '{userEmail}'
                        ORDER BY ci.DateAdded";
                    
                    var command = new SqlCommand(sql, connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new CartItem
                            {
                                Id = (int)reader["Id"],
                                UserId = reader["UserId"].ToString()!,
                                ItemName = reader["ItemName"].ToString()!,
                                Price = (decimal)reader["AdjustedPrice"], // Use adjusted price
                                Quantity = (int)reader["Quantity"],
                                DateAdded = (DateTime)reader["DateAdded"]
                            };
                            cartItems.Add(item);
                            subtotal += item.Price * item.Quantity;
                        }
                    }
                }

                if (!cartItems.Any())
                {
                    WriteToLogFile($"[{DateTime.Now}] CHECKOUT FAILED: Empty cart for {userEmail}");
                    return RedirectToAction("Index");
                }

                WriteToLogFile($"[{DateTime.Now}] Cart loaded: {cartItems.Count} items, subtotal: {subtotal:C}");

                // Complex business rules hard-coded in controller
                var isVipCustomer = subtotal > 1000;
                var isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
                var isBusinessHours = GlobalUtilities.IsBusinessHours();
                var isHolidaySeason = DateTime.Now.Month == 12;

                // Weekend surcharge - business logic mixed with infrastructure
                if (isWeekend && !isVipCustomer)
                {
                    subtotal *= 1.05m; // 5% weekend surcharge
                    WriteToLogFile($"[{DateTime.Now}] SURCHARGE: Weekend surcharge applied to {userEmail}");
                }

                // Holiday season bonus - more business logic
                if (isHolidaySeason)
                {
                    subtotal *= 0.95m; // 5% holiday discount
                    WriteToLogFile($"[{DateTime.Now}] DISCOUNT: Holiday discount applied to {userEmail}");
                }

                // Check order value limits
                if (subtotal > maxOrderValue)
                {
                    WriteToLogFile($"[{DateTime.Now}] ORDER LIMIT EXCEEDED: {userEmail} attempted {subtotal:C} > {maxOrderValue:C}");
                    SendEmail("Order Rejected", $"Your order of {subtotal:C} exceeds our limit of {maxOrderValue:C}");
                    return RedirectToAction("CartError");
                }

                // Manager approval for large orders - workflow logic in controller
                if (requiresManagerApproval && subtotal > 2000)
                {
                    // Create approval request file - file system dependency
                    var approvalFile = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "approvals", $"approval_{userId}_{DateTime.Now:yyyyMMddHHmmss}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(approvalFile)!);
                    
                    var approvalRequest = new
                    {
                        UserId = userId,
                        UserEmail = userEmail,
                        OrderAmount = subtotal,
                        RequestedAt = DateTime.Now,
                        Items = cartItems.Select(i => new { i.ItemName, i.Quantity, i.Price }).ToArray()
                    };
                    
                    System.IO.File.WriteAllText(approvalFile, JsonSerializer.Serialize(approvalRequest, new JsonSerializerOptions { WriteIndented = true }));
                    WriteToLogFile($"[{DateTime.Now}] APPROVAL REQUIRED: Request saved for {userEmail}");
                    
                    // Send approval email to manager - hard-coded recipient
                    SendManagerApprovalEmail(userEmail, subtotal, approvalFile);
                    return RedirectToAction("PendingApproval"); // Order pending approval
                }

                // Multiple external service calls - all blocking and synchronous
                decimal taxRate = 0;
                decimal shippingCost = 0;
                string courierService = "";
                
                // Tax service call with retry logic - but still blocking
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        WriteToLogFile($"[{DateTime.Now}] TAX SERVICE: Attempt {attempt} for {userEmail}");
                        var httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(10);
                        
                        var taxRequest = new
                        {
                            address = details.customerAddress,
                            amount = subtotal,
                            customerType = isVipCustomer ? "VIP" : "Standard",
                            items = cartItems.Select(i => new { name = i.ItemName, category = "General" }).ToArray()
                        };
                        
                        var content = new StringContent(JsonSerializer.Serialize(taxRequest), Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync($"{TAX_SERVICE_URL}/calculateTax", content).Result;
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var taxResponse = response.Content.ReadAsStringAsync().Result;
                            var taxData = JsonSerializer.Deserialize<dynamic>(taxResponse);
                            taxRate = Convert.ToDecimal(taxData?.GetProperty("rate").GetDecimal() ?? 0.08m);
                            WriteToLogFile($"[{DateTime.Now}] TAX RATE: {taxRate:P} for {userEmail}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToLogFile($"[{DateTime.Now}] TAX SERVICE ERROR (Attempt {attempt}): {ex.Message}");
                        if (attempt == 3)
                        {
                            taxRate = isVipCustomer ? 0.05m : 0.08m; // Fallback rates
                            WriteToLogFile($"[{DateTime.Now}] TAX FALLBACK: Using {taxRate:P} for {userEmail}");
                        }
                        Thread.Sleep(1000 * attempt); // Linear backoff - blocking
                    }
                }

                // Shipping service call - another external dependency
                try
                {
                    WriteToLogFile($"[{DateTime.Now}] SHIPPING SERVICE: Calculating for {userEmail}");
                    var shippingClient = new HttpClient();
                    shippingClient.Timeout = TimeSpan.FromSeconds(15);
                    
                    var shippingRequest = new
                    {
                        destination = details.customerAddress,
                        weight = cartItems.Sum(i => i.Quantity) * 0.5, // Assume 0.5 lbs per item
                        priority = details.shippingOption,
                        isVip = isVipCustomer
                    };
                    
                    var shippingContent = new StringContent(JsonSerializer.Serialize(shippingRequest), Encoding.UTF8, "application/json");
                    var shippingResponse = shippingClient.PostAsync("http://shipping.company.com/api/calculate", shippingContent).Result;
                    
                    if (shippingResponse.IsSuccessStatusCode)
                    {
                        var shippingData = JsonSerializer.Deserialize<dynamic>(shippingResponse.Content.ReadAsStringAsync().Result);
                        shippingCost = Convert.ToDecimal(shippingData?.GetProperty("cost").GetDecimal() ?? 10m);
                        courierService = shippingData?.GetProperty("courier").GetString() ?? "Standard";
                    }
                    else
                    {
                        // Hard-coded fallback shipping costs
                        shippingCost = details.shippingOption == "Express" ? (isVipCustomer ? 20 : 25) : (isVipCustomer ? 5 : 10);
                        courierService = details.shippingOption == "Express" ? "FedEx" : "USPS";
                    }
                    
                    WriteToLogFile($"[{DateTime.Now}] SHIPPING: {shippingCost:C} via {courierService} for {userEmail}");
                }
                catch (Exception ex)
                {
                    WriteToLogFile($"[{DateTime.Now}] SHIPPING ERROR: {ex.Message}");
                    shippingCost = details.shippingOption == "Express" ? 25 : 10; // Hard-coded fallback
                    courierService = "Standard";
                }

                // Calculate final total with complex business rules
                var taxAmount = subtotal * taxRate;
                var totalPrice = subtotal + taxAmount + shippingCost;
                
                // VIP discount applied after taxes - more business logic
                if (isVipCustomer && totalPrice > 500)
                {
                    var vipDiscount = totalPrice * 0.02m; // 2% VIP discount
                    totalPrice -= vipDiscount;
                    WriteToLogFile($"[{DateTime.Now}] VIP DISCOUNT: {vipDiscount:C} applied to {userEmail}");
                }

                WriteToLogFile($"[{DateTime.Now}] PRICING: Subtotal={subtotal:C}, Tax={taxAmount:C}, Shipping={shippingCost:C}, Total={totalPrice:C}");

                // Inventory management via XML files - file system coupling
                foreach (var item in cartItems)
                {
                    try
                    {
                        UpdateInventoryFile(item.ItemName, item.Quantity);
                        
                        // Also log to separate inventory tracking file
                        var inventoryLogFile = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "inventory_transactions.log");
                        var inventoryEntry = $"[{DateTime.Now}] CHECKOUT: -{item.Quantity} {item.ItemName} for order by {userEmail}";
                        System.IO.File.AppendAllText(inventoryLogFile, inventoryEntry + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        WriteToLogFile($"[{DateTime.Now}] INVENTORY ERROR: {ex.Message} for item {item.ItemName}");
                        // Continue processing even if inventory update fails
                    }
                }

                // Payment processing with multiple attempts and different payment methods
                string paymentResult = "";
                string paymentMethod = totalPrice > 1000 ? "CreditCard" : "DebitCard";
                
                for (int paymentAttempt = 1; paymentAttempt <= 2; paymentAttempt++)
                {
                    try
                    {
                        WriteToLogFile($"[{DateTime.Now}] PAYMENT: Attempt {paymentAttempt} using {paymentMethod} for {userEmail}");
                        
                        var paymentProcessor = new PaymentProcessor();
                        
                        // Different payment logic based on amount and customer type
                        if (isVipCustomer && totalPrice > 2000)
                        {
                            // VIP customers get special payment processing
                            paymentResult = paymentProcessor.ProcessVipPayment(totalPrice, userEmail);
                        }
                        else if (totalPrice > 1000)
                        {
                            // Large orders get different processing
                            paymentResult = paymentProcessor.ProcessLargePayment(totalPrice);
                        }
                        else
                        {
                            // Standard payment processing
                            paymentResult = paymentProcessor.ProcessPayment(totalPrice);
                        }
                        
                        WriteToLogFile($"[{DateTime.Now}] PAYMENT RESULT: {paymentResult} for {userEmail}");
                        
                        if (paymentResult == "Approved")
                        {
                            break;
                        }
                        else if (paymentResult == "Declined" && paymentAttempt == 1)
                        {
                            // Try different payment method on first failure
                            paymentMethod = "CreditCard";
                            WriteToLogFile($"[{DateTime.Now}] PAYMENT: Retrying with {paymentMethod} for {userEmail}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToLogFile($"[{DateTime.Now}] PAYMENT EXCEPTION: {ex.Message}");
                        paymentResult = "Error";
                    }
                }

                if (paymentResult != "Approved")
                {
                    WriteToLogFile($"[{DateTime.Now}] PAYMENT FAILED: {paymentResult} for {userEmail}");
                    
                    // Send failure notification with detailed information
                    var failureEmail = $@"
Payment Failed

Dear Customer,

Your payment of {totalPrice:C} was {paymentResult.ToLower()}.

Order Details:
- Items: {cartItems.Count}
- Subtotal: {subtotal:C}
- Tax: {taxAmount:C}
- Shipping: {shippingCost:C}
- Total: {totalPrice:C}
- Payment Method: {paymentMethod}

Please try again or contact customer service.

Best regards,
WebShop Team";
                    
                    SendEmail("Payment Failed", failureEmail);
                    
                    // Also save failure details to file for analysis
                    var failureFile = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "payment_failures", $"failure_{userId}_{DateTime.Now:yyyyMMddHHmmss}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(failureFile)!);
                    
                    var failureData = new
                    {
                        UserId = userId,
                        UserEmail = userEmail,
                        Amount = totalPrice,
                        Result = paymentResult,
                        PaymentMethod = paymentMethod,
                        Timestamp = DateTime.Now,
                        CartItems = cartItems.Count,
                        IsVip = isVipCustomer
                    };
                    
                    System.IO.File.WriteAllText(failureFile, JsonSerializer.Serialize(failureData, new JsonSerializerOptions { WriteIndented = true }));
                    
                    return RedirectToAction("PaymentFailed");
                }

                // Create order using direct SQL instead of EF for "performance" - more tight coupling
                int orderId = 0;
                using (var connection = new SqlConnection(DB_CONNECTION))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert order with SQL injection vulnerability
                            var orderSql = $@"
                                INSERT INTO Orders (UserId, CustomerAddress, ShippingOption, TotalAmount, CreatedDate, CourierService, PaymentMethod, IsVipOrder)
                                VALUES ('{userId}', '{details.customerAddress}', '{details.shippingOption}', {totalPrice}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', '{courierService}', '{paymentMethod}', {(isVipCustomer ? 1 : 0)});
                                SELECT SCOPE_IDENTITY();";
                            
                            var orderCommand = new SqlCommand(orderSql, connection, transaction);
                            orderId = Convert.ToInt32(orderCommand.ExecuteScalar());
                            
                            WriteToLogFile($"[{DateTime.Now}] ORDER CREATED: {orderId} for {userEmail}");

                            // Insert order items
                            foreach (var item in cartItems)
                            {
                                var itemSql = $@"
                                    INSERT INTO OrderItems (OrderId, ItemName, Price, Quantity)
                                    VALUES ({orderId}, '{item.ItemName}', {item.Price}, {item.Quantity})";
                                
                                var itemCommand = new SqlCommand(itemSql, connection, transaction);
                                itemCommand.ExecuteNonQuery();
                            }

                            // Clear cart items
                            var clearCartSql = $"DELETE FROM CartItems WHERE UserId = '{userId}'";
                            var clearCommand = new SqlCommand(clearCartSql, connection, transaction);
                            clearCommand.ExecuteNonQuery();

                            transaction.Commit();
                            WriteToLogFile($"[{DateTime.Now}] ORDER COMMITTED: {orderId} for {userEmail}");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            WriteToLogFile($"[{DateTime.Now}] ORDER ROLLBACK: {ex.Message}");
                            throw;
                        }
                    }
                }

                // Post-order processing - multiple external service calls
                
                // Analytics tracking
                SendAnalyticsData(userEmail, "Checkout", $"Order-{orderId}", cartItems.Count, totalPrice);
                
                // Warehouse notification for fulfillment
                try
                {
                    var warehouseClient = new HttpClient();
                    var warehouseData = new
                    {
                        orderId = orderId,
                        customerEmail = userEmail,
                        items = cartItems.Select(i => new { name = i.ItemName, quantity = i.Quantity }).ToArray(),
                        shippingAddress = details.customerAddress,
                        courierService = courierService,
                        priority = isVipCustomer ? "High" : "Normal"
                    };
                    
                    var warehouseContent = new StringContent(JsonSerializer.Serialize(warehouseData), Encoding.UTF8, "application/json");
                    var warehouseResponse = warehouseClient.PostAsync("http://warehouse.company.com/api/fulfill", warehouseContent).Result;
                    
                    WriteToLogFile($"[{DateTime.Now}] WAREHOUSE: Notification sent for order {orderId}");
                }
                catch (Exception ex)
                {
                    WriteToLogFile($"[{DateTime.Now}] WAREHOUSE ERROR: {ex.Message}");
                }

                // CRM system update
                try
                {
                    var crmClient = new HttpClient();
                    var crmData = new
                    {
                        customerEmail = userEmail,
                        orderValue = totalPrice,
                        orderDate = DateTime.Now,
                        isVip = isVipCustomer,
                        itemsPurchased = cartItems.Count
                    };
                    
                    var crmContent = new StringContent(JsonSerializer.Serialize(crmData), Encoding.UTF8, "application/json");
                    var crmResponse = crmClient.PostAsync("http://crm.company.com/api/update-customer", crmContent).Result;
                    
                    WriteToLogFile($"[{DateTime.Now}] CRM: Customer data updated for {userEmail}");
                }
                catch (Exception ex)
                {
                    WriteToLogFile($"[{DateTime.Now}] CRM ERROR: {ex.Message}");
                }

                // Save order summary to file for backup
                var orderSummary = new
                {
                    OrderId = orderId,
                    CustomerEmail = userEmail,
                    Subtotal = subtotal,
                    Tax = taxAmount,
                    Shipping = shippingCost,
                    Total = totalPrice,
                    Items = cartItems.Select(i => new { i.ItemName, i.Quantity, i.Price }).ToArray(),
                    ProcessedAt = DateTime.Now,
                    CourierService = courierService,
                    PaymentMethod = paymentMethod,
                    IsVip = isVipCustomer
                };
                
                var orderSummaryFile = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "order_summaries", $"order_{orderId}_{DateTime.Now:yyyyMMdd}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(orderSummaryFile)!);
                System.IO.File.WriteAllText(orderSummaryFile, JsonSerializer.Serialize(orderSummary, new JsonSerializerOptions { WriteIndented = true }));

                // Send detailed confirmation email
                var confirmationEmail = $@"
Order Confirmation - #{orderId}

Dear {userEmail.Split('@')[0]},

Thank you for your order! Here are the details:

Order Number: {orderId}
Order Date: {DateTime.Now:yyyy-MM-dd HH:mm}
Customer Status: {(isVipCustomer ? "VIP Customer" : "Standard Customer")}

Items Ordered:
{string.Join("\n", cartItems.Select(i => $"- {i.ItemName} x{i.Quantity} @ {i.Price:C} = {i.Price * i.Quantity:C}"))}

Pricing Breakdown:
- Subtotal: {subtotal:C}
- Tax ({taxRate:P}): {taxAmount:C}
- Shipping via {courierService}: {shippingCost:C}
- TOTAL: {totalPrice:C}

Shipping Information:
- Method: {details.shippingOption}
- Courier: {courierService}
- Address: {details.customerAddress}
- Estimated Delivery: {(details.shippingOption == "Express" ? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") : DateTime.Now.AddDays(3).ToString("yyyy-MM-dd"))}

Payment Method: {paymentMethod}

{(isVipCustomer ? "As a VIP customer, you've received priority processing and special discounts!" : "Thank you for choosing our service!")}

You can track your order at: http://tracking.company.com/order/{orderId}

Best regards,
The WebShop Team

Order processed on {Environment.MachineName} at {DateTime.Now}
";

                SendEmail($"Order Confirmation #{orderId}", confirmationEmail);

                WriteToLogFile($"[{DateTime.Now}] CHECKOUT COMPLETE: Order {orderId} for {userEmail} - Total: {totalPrice:C}");
                
                // Update global statistics
                GlobalUtilities.ErrorCount = 0; // Reset error count on successful order

                // Create and store confirmation view model
                var confirmationViewModel = new OrderConfirmationViewModel
                {
                    OrderId = orderId,
                    CustomerEmail = userEmail,
                    CustomerAddress = details.customerAddress,
                    ShippingOption = details.shippingOption,
                    Courier = courierService,
                    EstimatedDeliveryDate = details.shippingOption == "Express"
                        ? DateTime.Now.AddDays(1)
                        : DateTime.Now.AddDays(3),
                    Subtotal = subtotal,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    ShippingCost = shippingCost,
                    TotalPrice = totalPrice,
                    Items = cartItems.Select(i => new CartItemViewModel
                    {
                        ItemName = i.ItemName,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };

                TempData["OrderConfirmation"] = JsonSerializer.Serialize(
                    confirmationViewModel,
                    new JsonSerializerOptions { WriteIndented = false });

                return RedirectToAction("OrderConfirmation");
            }
            catch (Exception ex)
            {
                var userEmail = User.Identity?.Name ?? "Unknown";
                WriteToLogFile($"[{DateTime.Now}] CHECKOUT EXCEPTION: {ex.Message} for {userEmail}");
                
                // Send detailed error report to admin
                SendErrorNotificationToWarehouse(userEmail, $"Checkout failed: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                
                // Also save error details for debugging
                var errorFile = Path.Combine(GlobalUtilities.LOG_DIRECTORY, $"checkout_errors_{DateTime.Now:yyyy-MM-dd}.log");
                var errorDetails = $"[{DateTime.Now}] CHECKOUT ERROR for {userEmail}:\n{ex}\n\n";
                System.IO.File.AppendAllText(errorFile, errorDetails);
                
                throw; // Re-throw for controller error handling
            }
        }

        public IActionResult OrderConfirmation()
        {
            if (TempData["OrderConfirmation"] is string json)
            {
                var model = JsonSerializer.Deserialize<OrderConfirmationViewModel>(json);
                return View(model);
            }

            TempData["ErrorMessage"] = "Order confirmation details not found.";
            return RedirectToAction("Index");
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
