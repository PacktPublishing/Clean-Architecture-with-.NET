using Domain.Enums;

namespace Domain.Entities;

public class Order(Guid userId, List<OrderItem> items, decimal totalAmount)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; } = userId;
    public List<OrderItem> Items { get; private set; } = items;
    public decimal TotalAmount { get; private set; } = totalAmount;
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending; // Initial status
}