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
        builder.Property(product => product.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(product => product.StockLevel).IsRequired();
        builder.Property(product => product.ImageUrl).IsRequired().HasMaxLength(4000);

        // Seed data (only included in this configuration for demonstration purposes)
        builder.HasData(
            new Product
            {
                Id = Guid.Parse("6368b5f7-93f6-44a9-961b-f6d7130dd715"),
                Name = "Wireless Mouse",
                Price = 29.99m,
                StockLevel = 150,
                ImageUrl = "https://placehold.co/200?text=Wireless+Mouse"
            },
            new Product
            {
                Id = Guid.Parse("44de0563-24ec-4dff-b263-9ce004ba1fff"),
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                StockLevel = 75,
                ImageUrl = "https://placehold.co/200?text=Mechanical+Keyboard"
            },
            new Product
            {
                Id = Guid.Parse("b10d1e56-bfc3-46ca-916d-0bb0b4bc087d"),
                Name = "27-inch Monitor",
                Price = 199.99m,
                StockLevel = 45,
                ImageUrl = "https://placehold.co/200?text=27-inch+Monitor"
            },
            new Product
            {
                Id = Guid.Parse("ce2c68e7-2911-49bb-88aa-67870967f50b"),
                Name = "USB-C Hub",
                Price = 49.99m,
                StockLevel = 200,
                ImageUrl = "https://placehold.co/200?text=USB-C+Hub"
            },
            new Product
            {
                Id = Guid.Parse("dfcc155e-49c0-4bc8-b3ee-d9c056059609"),
                Name = "Noise Cancelling Headphones",
                Price = 129.99m,
                StockLevel = 30,
                ImageUrl = "https://placehold.co/200?text=Noise+Cancelling+Headphones"
            },
            new Product
            {
                Id = Guid.Parse("4e3cfa2d-870c-4249-92dd-e8865338f5c9"),
                Name = "Webcam",
                Price = 69.99m,
                StockLevel = 120,
                ImageUrl = "https://placehold.co/200?text=Webcam"
            },
            new Product
            {
                Id = Guid.Parse("b59bb18f-c4b8-4030-89e3-5694e5e67efa"),
                Name = "Portable SSD",
                Price = 99.99m,
                StockLevel = 80,
                ImageUrl = "https://placehold.co/200?text=Portable+SSD"
            },
            new Product
            {
                Id = Guid.Parse("9077d14d-c6e5-42f1-b56c-89c53363addf"),
                Name = "Gaming Chair",
                Price = 249.99m,
                StockLevel = 20,
                ImageUrl = "https://placehold.co/200?text=Gaming+Chair"
            },
            new Product
            {
                Id = Guid.Parse("070a1995-d1f6-4e0d-b16f-f5bf5b362529"),
                Name = "Smart Speaker",
                Price = 39.99m,
                StockLevel = 90,
                ImageUrl = "https://placehold.co/200?text=Smart+Speaker"
            }
        );
    }
}