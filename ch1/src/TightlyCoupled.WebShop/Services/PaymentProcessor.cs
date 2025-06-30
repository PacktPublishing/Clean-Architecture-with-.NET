namespace TightlyCoupled.WebShop.Services
{
    /// <summary>
    /// Dummy PaymentProcessor class - demonstrating bad practices with multiple hard-coded payment methods
    /// </summary>
    public class PaymentProcessor
    {
        public string ProcessPayment(decimal amount)
        {
            // Simulated payment processing logic
            if (amount > 1000)
            {
                return "Declined";
            }
            return "Approved";
        }
        
        // Hard-coded VIP payment processing - bad practice
        public string ProcessVipPayment(decimal amount, string customerEmail)
        {
            // VIP customers get special treatment with hard-coded rules
            if (amount > 5000)
            {
                return "Pending"; // Manual review required
            }
            
            // VIP customers rarely get declined
            if (amount > 3000 && DateTime.Now.Hour < 6)
            {
                return "Declined"; // Outside business hours
            }
            
            return "Approved";
        }
        
        // Hard-coded large payment processing
        public string ProcessLargePayment(decimal amount)
        {
            // Different logic for large payments
            if (amount > 2000 && DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                return "Pending"; // Manual review on Sundays
            }
            
            if (amount > 4000)
            {
                return "Declined"; // Too large for automatic processing
            }
            
            return "Approved";
        }
    }
}
