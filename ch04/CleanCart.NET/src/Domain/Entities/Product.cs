using System;

namespace Domain.Entities;

public class Product(string name, decimal price)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = name;
    public decimal Price { get; private set; } = price;
}
