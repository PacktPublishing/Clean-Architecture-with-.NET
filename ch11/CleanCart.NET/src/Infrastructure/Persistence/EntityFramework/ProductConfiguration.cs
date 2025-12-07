using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityFramework;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).IsRequired().HasMaxLength(255);
        builder.Property(product => product.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(product => product.StockLevel).IsRequired();
        builder.Property(product => product.ImageUrl).IsRequired().HasMaxLength(4000);

        // Seed data (only included in this configuration for demonstration purposes)
        builder.HasData(
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Price = 29.99m,
                StockLevel = 150,
                ImageUrl = "https://placehold.co/200?text=Wireless+Mouse"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                StockLevel = 75,
                ImageUrl = "https://placehold.co/200?text=Mechanical+Keyboard"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "27-inch Monitor",
                Price = 199.99m,
                StockLevel = 45,
                ImageUrl = "https://placehold.co/200?text=27-inch+Monitor"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "USB-C Hub",
                Price = 49.99m,
                StockLevel = 200,
                ImageUrl = "https://placehold.co/200?text=USB-C+Hub"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Noise Cancelling Headphones",
                Price = 129.99m,
                StockLevel = 30,
                ImageUrl = "https://placehold.co/200?text=Noise+Cancelling+Headphones"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Webcam",
                Price = 69.99m,
                StockLevel = 120,
                ImageUrl = "https://placehold.co/200?text=Webcam"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Portable SSD",
                Price = 99.99m,
                StockLevel = 80,
                ImageUrl = "https://placehold.co/200?text=Portable+SSD"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Gaming Chair",
                Price = 249.99m,
                StockLevel = 20,
                ImageUrl = "https://placehold.co/200?text=Gaming+Chair"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Smart Speaker",
                Price = 39.99m,
                StockLevel = 90,
                ImageUrl = "https://placehold.co/200?text=Smart+Speaker"
            }
        );
    }
}