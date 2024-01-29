using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedOn { get; set; }
    public string Status { get; set; }
    public User NavUser { get; set; }
}