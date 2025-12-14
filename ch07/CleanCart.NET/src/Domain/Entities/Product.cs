namespace Domain.Entities;

public class Product(string name, decimal price, int stockLevel)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; } = name;
    public decimal Price { get; } = price;
    public int StockLevel { get; private set; } = stockLevel;

    public void UpdateStockLevel(int stockLevel)
    {
        if (stockLevel < 0)
        {
            throw new ArgumentException("Stock level cannot be negative", nameof(stockLevel));
        }

        StockLevel = stockLevel;
    }
}