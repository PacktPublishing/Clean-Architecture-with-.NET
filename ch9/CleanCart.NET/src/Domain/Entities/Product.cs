using System;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int StockLevel { get; private set; }
        public string ImageUrl { get; }

        public Product(Guid id, string name, decimal price, int stockLevel, string imageUrl)
        {
            Id = id;
            Name = name;
            Price = price;
            StockLevel = stockLevel;
            ImageUrl = imageUrl;
        }

        public void UpdateStockLevel(int stockLevel)
        {
            if (stockLevel < 0)
            {
                throw new ArgumentException("Stock level cannot be negative", nameof(stockLevel));
            }

            StockLevel = stockLevel;
        }
    }
}
