using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Entities;

public class ShoppingCart : IEntityId<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; }
    public User NavUser { get; set; }
}