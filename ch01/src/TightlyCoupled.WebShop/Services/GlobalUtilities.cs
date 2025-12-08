using System;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace TightlyCoupled.WebShop.Services
{
    /// <summary>
    /// Static utility class that violates dependency injection principles
    /// and creates tight coupling throughout the application
    /// </summary>
    public static class GlobalUtilities
    {
        // Hard-coded paths and configuration
        public const string LOG_DIRECTORY = @"C:\Logs\WebShop\";
        public const string DATA_DIRECTORY = @"C:\Data\WebShop\";
        public const string CONFIG_FILE = @"C:\Config\webshop.config";
        public const string DATABASE_CONNECTION = "Server=(localdb)\\mssqllocaldb;Database=TightlyCoupledWebShop;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        // Global state - very bad practice
        public static string CurrentUser { get; set; } = "";
        public static DateTime LastActivity { get; set; } = DateTime.Now;
        public static int ErrorCount { get; set; } = 0;
        
        // File system dependency in static method
        public static void LogError(string message)
        {
            try
            {
                var logFile = Path.Combine(LOG_DIRECTORY, $"errors_{DateTime.Now:yyyy-MM-dd}.log");
                Directory.CreateDirectory(LOG_DIRECTORY);
                File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR: {message}{Environment.NewLine}");
                ErrorCount++;
            }
            catch
            {
                // Swallow exceptions
            }
        }
        
        // Direct database access in utility method
        public static string GetUserDisplayName(string userId)
        {
            try
            {
                using var connection = new SqlConnection(DATABASE_CONNECTION);
                connection.Open();
                var command = new SqlCommand($"SELECT Email FROM AspNetUsers WHERE Id = '{userId}'", connection);
                return command.ExecuteScalar()?.ToString() ?? "Unknown User";
            }
            catch
            {
                return "Error Loading User";
            }
        }
        
        // Business logic in utility class
        public static bool IsBusinessHours()
        {
            var hour = DateTime.Now.Hour;
            return hour >= 9 && hour <= 17 && DateTime.Now.DayOfWeek != DayOfWeek.Sunday;
        }
        
        // Configuration tightly coupled to file system
        public static string GetConfigValue(string key)
        {
            try
            {
                var lines = File.ReadAllLines(CONFIG_FILE);
                foreach (var line in lines)
                {
                    if (line.StartsWith(key + "="))
                    {
                        return line.Substring(key.Length + 1);
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
        
        // Environment-specific code mixed in
        public static void CreateRequiredDirectories()
        {
            try
            {
                Directory.CreateDirectory(LOG_DIRECTORY);
                Directory.CreateDirectory(DATA_DIRECTORY);
                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE)!);
                
                // Create default config if not exists
                if (!File.Exists(CONFIG_FILE))
                {
                    File.WriteAllText(CONFIG_FILE, @"admin_email=admin@company.com
smtp_server=smtp.company.com
smtp_port=587
max_cart_items=10
session_timeout=30
enable_analytics=true
warehouse_api=http://warehouse.company.com/api
tax_service=http://taxservice.company.com");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create directories: {ex.Message}");
            }
        }
    }
}
