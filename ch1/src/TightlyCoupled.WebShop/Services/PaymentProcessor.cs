namespace TightlyCoupled.WebShop.Services
{
    /// <summary>
    /// Dummy PaymentProcessor class
    /// </summary>
    public class PaymentProcessor
    {
        public string ProcessPayment(double amount)
        {
            // Simulated payment processing logic
            if (amount > 1000)
            {
                return "Declined";
            }
            return "Approved";
        }
    }
}
