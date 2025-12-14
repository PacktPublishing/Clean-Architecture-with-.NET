using Domain.Enums;
using EntityAxis.Abstractions;

namespace Domain.Entities;

public class Order : IEntityId<Guid>
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public OrderStatus Status { get; set; }

    public Order(Guid userId, IEnumerable<OrderItem> items, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        _items.AddRange(items);
        TotalAmount = totalAmount;
        CreatedOn = DateTime.UtcNow;
        Status = OrderStatus.Pending; // Initial status
    }
}
