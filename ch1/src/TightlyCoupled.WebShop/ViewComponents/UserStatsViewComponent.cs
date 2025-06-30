using Microsoft.AspNetCore.Mvc;
using TightlyCoupled.WebShop.Services;
using TightlyCoupled.WebShop.Data;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace TightlyCoupled.WebShop.ViewComponents
{
    /// <summary>
    /// View component that demonstrates tight coupling by mixing 
    /// presentation logic with business logic, data access, and external services
    /// </summary>
    public class UserStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        
        public UserStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IViewComponentResult Invoke()
        {
            try
            {
                // Update global state - bad practice
                GlobalUtilities.LastActivity = DateTime.Now;
                GlobalUtilities.CurrentUser = User.Identity?.Name ?? "Anonymous";
                
                var userEmail = User.Identity?.Name ?? "Anonymous";
                var stats = new UserStatsViewModel();
                
                // Direct SQL queries with string concatenation - SQL injection risk
                using (var connection = new SqlConnection(GlobalUtilities.DATABASE_CONNECTION))
                {
                    connection.Open();
                    
                    // Get cart count
                    var cartCommand = new SqlCommand($"SELECT COUNT(*) FROM CartItems WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '{userEmail}')", connection);
                    stats.CartItemCount = (int)cartCommand.ExecuteScalar();
                    
                    // Get order count
                    var orderCommand = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '{userEmail}')", connection);
                    stats.OrderCount = (int)orderCommand.ExecuteScalar();
                    
                    // Get total spent
                    var spentCommand = new SqlCommand($"SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '{userEmail}')", connection);
                    stats.TotalSpent = (decimal)spentCommand.ExecuteScalar();
                }
                
                // Call external service for user recommendations - blocking call
                try
                {
                    var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    
                    var requestData = new { user = userEmail, timestamp = DateTime.Now };
                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = httpClient.PostAsync("http://recommendations.company.com/api/get-recommendations", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var recommendationsJson = response.Content.ReadAsStringAsync().Result;
                        var recommendations = JsonSerializer.Deserialize<string[]>(recommendationsJson);
                        stats.Recommendations = recommendations ?? new string[0];
                    }
                }
                catch
                {
                    stats.Recommendations = new[] { "Featured Item 1", "Featured Item 2" }; // Fallback
                }
                
                // Business logic in view component
                if (stats.TotalSpent > 1000)
                {
                    stats.UserTier = "Premium";
                    stats.Discount = 0.10m;
                }
                else if (stats.TotalSpent > 500)
                {
                    stats.UserTier = "Gold";
                    stats.Discount = 0.05m;
                }
                else
                {
                    stats.UserTier = "Standard";
                    stats.Discount = 0m;
                }
                
                // Check business hours using global utility
                stats.IsBusinessHours = GlobalUtilities.IsBusinessHours();
                
                // File system access for personalization
                var personalizedGreeting = GetPersonalizedGreeting(userEmail);
                stats.Greeting = personalizedGreeting;
                
                // Log to file system
                GlobalUtilities.LogError($"UserStats viewed by {userEmail} - Cart: {stats.CartItemCount}, Orders: {stats.OrderCount}");
                
                return View(stats);
            }
            catch (Exception ex)
            {
                GlobalUtilities.ErrorCount++;
                GlobalUtilities.LogError($"UserStatsViewComponent error: {ex.Message}");
                
                // Return fallback view with hardcoded data
                return View(new UserStatsViewModel 
                { 
                    CartItemCount = 0, 
                    OrderCount = 0, 
                    TotalSpent = 0, 
                    UserTier = "Standard",
                    Greeting = "Welcome!",
                    Recommendations = new[] { "Default Item" }
                });
            }
        }
        
        // File system dependency for user personalization
        private string GetPersonalizedGreeting(string userEmail)
        {
            try
            {
                var greetingFile = Path.Combine(GlobalUtilities.DATA_DIRECTORY, "greetings", $"{userEmail.Replace("@", "_").Replace(".", "_")}.txt");
                
                if (File.Exists(greetingFile))
                {
                    return File.ReadAllText(greetingFile);
                }
                else
                {
                    // Create personalized greeting based on time and user data
                    var hour = DateTime.Now.Hour;
                    var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
                    
                    var personalizedGreeting = $"{greeting}, {userEmail.Split('@')[0]}!";
                    
                    // Save to file for next time
                    Directory.CreateDirectory(Path.GetDirectoryName(greetingFile)!);
                    File.WriteAllText(greetingFile, personalizedGreeting);
                    
                    return personalizedGreeting;
                }
            }
            catch
            {
                return "Welcome!";
            }
        }
    }
    
    public class UserStatsViewModel
    {
        public int CartItemCount { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public string UserTier { get; set; } = "";
        public decimal Discount { get; set; }
        public bool IsBusinessHours { get; set; }
        public string Greeting { get; set; } = "";
        public string[] Recommendations { get; set; } = new string[0];
    }
}
