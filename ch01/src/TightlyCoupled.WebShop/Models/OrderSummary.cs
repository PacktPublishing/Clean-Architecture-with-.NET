using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using TightlyCoupled.WebShop.Services;

namespace TightlyCoupled.WebShop.Models
{
    /// <summary>
    /// Model class that violates single responsibility principle
    /// by containing business logic, data access, and external service calls
    /// </summary>
    public class OrderSummary
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public List<OrderItemSummary> Items { get; set; } = new();
        
        // Business logic in model - bad practice
        public string GetFormattedTotal()
        {
            return TotalAmount.ToString("C");
        }
        
        public string GetStatusDisplay()
        {
            return Status switch
            {
                "P" => "Pending",
                "S" => "Shipped",
                "D" => "Delivered",
                "C" => "Cancelled",
                _ => "Unknown"
            };
        }
        
        public bool IsRecentOrder()
        {
            return (DateTime.Now - OrderDate).TotalDays <= 30;
        }
        
        // Data access in model - terrible practice
        public static List<OrderSummary> GetOrdersForUser(string userEmail)
        {
            var orders = new List<OrderSummary>();
            
            try
            {
                using var connection = new SqlConnection(GlobalUtilities.DATABASE_CONNECTION);
                connection.Open();
                
                // SQL injection vulnerability
                var sql = $@"
                    SELECT o.Id, o.UserId, o.TotalAmount, o.CreatedDate, 
                           CASE WHEN o.CreatedDate > DATEADD(day, -7, GETDATE()) THEN 'P'
                                WHEN o.CreatedDate > DATEADD(day, -14, GETDATE()) THEN 'S'
                                ELSE 'D' END as Status,
                           o.CustomerAddress
                    FROM Orders o
                    INNER JOIN AspNetUsers u ON o.UserId = u.Id
                    WHERE u.Email = '{userEmail}'
                    ORDER BY o.CreatedDate DESC";
                
                var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var order = new OrderSummary
                    {
                        OrderId = (int)reader["Id"],
                        UserId = reader["UserId"].ToString()!,
                        TotalAmount = (decimal)reader["TotalAmount"],
                        OrderDate = (DateTime)reader["CreatedDate"],
                        Status = reader["Status"].ToString()!,
                        ShippingAddress = reader["CustomerAddress"].ToString()!
                    };
                    
                    orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                GlobalUtilities.LogError($"Failed to get orders for {userEmail}: {ex.Message}");
            }
            
            // Load items for each order
            foreach (var order in orders)
            {
                order.LoadOrderItems();
            }
            
            return orders;
        }
        
        // More data access in model
        public void LoadOrderItems()
        {
            try
            {
                using var connection = new SqlConnection(GlobalUtilities.DATABASE_CONNECTION);
                connection.Open();
                
                var sql = $"SELECT ItemName, Price, Quantity FROM OrderItems WHERE OrderId = {OrderId}";
                var command = new SqlCommand(sql, connection);
                using var reader = command.ExecuteReader();
                
                Items.Clear();
                while (reader.Read())
                {
                    Items.Add(new OrderItemSummary
                    {
                        ItemName = reader["ItemName"].ToString()!,
                        Price = (decimal)reader["Price"],
                        Quantity = (int)reader["Quantity"]
                    });
                }
            }
            catch (Exception ex)
            {
                GlobalUtilities.LogError($"Failed to load items for order {OrderId}: {ex.Message}");
            }
        }
        
        // External service call in model
        public string GetTrackingInfo()
        {
            if (Status != "S" && Status != "D") return "Not shipped yet";
            
            try
            {
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var trackingData = new
                {
                    orderId = OrderId,
                    customerEmail = GlobalUtilities.GetUserDisplayName(UserId)
                };
                
                var json = JsonSerializer.Serialize(trackingData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = httpClient.PostAsync("http://shipping.company.com/api/track", content).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                
                return "Tracking information not available";
            }
            catch
            {
                return "Error retrieving tracking information";
            }
        }
        
        // File system dependency in model
        public void SaveOrderSummaryToFile()
        {
            try
            {
                var fileName = $"order_{OrderId}_{DateTime.Now:yyyyMMdd}.json";
                var filePath = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "order_summaries", fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                
                var summary = new
                {
                    OrderId,
                    UserId,
                    TotalAmount,
                    OrderDate,
                    Status = GetStatusDisplay(),
                    Items = Items.Select(i => new { i.ItemName, i.Price, i.Quantity }).ToArray(),
                    GeneratedAt = DateTime.Now
                };
                
                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                
                GlobalUtilities.LogError($"Order summary saved to file for order {OrderId}");
            }
            catch (Exception ex)
            {
                GlobalUtilities.LogError($"Failed to save order summary to file: {ex.Message}");
            }
        }
        
        // Email functionality in model - mixing concerns
        public void SendOrderUpdateEmail()
        {
            try
            {
                var smtpServer = GlobalUtilities.GetConfigValue("smtp_server");
                var smtpPort = int.Parse(GlobalUtilities.GetConfigValue("smtp_port") ?? "587");
                var adminEmail = GlobalUtilities.GetConfigValue("admin_email");
                
                // Create email content
                var subject = $"Order Update - #{OrderId}";
                var body = $@"
Dear Customer,

Your order #{OrderId} has been updated.

Status: {GetStatusDisplay()}
Total Amount: {GetFormattedTotal()}
Order Date: {OrderDate:yyyy-MM-dd}

Items:
{string.Join("\n", Items.Select(i => $"- {i.ItemName} x{i.Quantity} @ {i.Price:C}"))}

Tracking Info: {GetTrackingInfo()}

Thank you for your business!

Best regards,
WebShop Team
";

                // This would normally use an email service, but for demo purposes
                // we'll just log it
                GlobalUtilities.LogError($"EMAIL SENT: {subject} to user {UserId}");
                
                // Save email to file system as backup
                var emailFileName = $"email_order_{OrderId}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                var emailPath = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "emails", emailFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(emailPath)!);
                File.WriteAllText(emailPath, $"To: {GlobalUtilities.GetUserDisplayName(UserId)}\nSubject: {subject}\n\n{body}");
            }
            catch (Exception ex)
            {
                GlobalUtilities.LogError($"Failed to send order update email: {ex.Message}");
            }
        }
    }
    
    public class OrderItemSummary
    {
        public string ItemName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        
        public decimal GetTotal() => Price * Quantity;
        public string GetFormattedTotal() => GetTotal().ToString("C");
    }
}
