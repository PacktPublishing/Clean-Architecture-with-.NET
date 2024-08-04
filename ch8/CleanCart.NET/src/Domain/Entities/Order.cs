using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public List<OrderItem> Items { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public OrderStatus Status { get; set; }

        public Order(Guid userId, List<OrderItem> items, decimal totalAmount)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Items = items;
            TotalAmount = totalAmount;
            CreatedOn = DateTime.UtcNow;
            Status = OrderStatus.Pending; // Initial status
        }
    }
}