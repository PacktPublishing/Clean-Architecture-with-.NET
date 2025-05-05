using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Entities;

public class User : IEntityId<Guid>
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public List<string> Roles { get; set; }
    public List<Order> NavOrders { get; set; }
    public ShoppingCart NavShoppingCart { get; set; }
}