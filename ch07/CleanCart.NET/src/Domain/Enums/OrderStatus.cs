namespace Domain.Enums;

public enum OrderStatus
{
    Pending,        // The initial status when the order is created
    PaymentFailed,  // Payment processing failed
    Paid,           // Payment successfully processed
    Shipped,        // Order has been shipped to the customer
    Delivered,      // Order has been delivered to the customer
    Cancelled       // Order was cancelled by the customer or admin
}