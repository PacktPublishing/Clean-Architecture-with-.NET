using Domain.Enums;
using EntityAxis.Abstractions;

namespace Domain.Entities;

public class Order(Guid userId, List<OrderItem> items, decimal totalAmount) : IEntityId<Guid>
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; } = userId;
    public List<OrderItem> Items { get; private set; } = items;
    public decimal TotalAmount { get; private set; } = totalAmount;
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending; // Initial status
}