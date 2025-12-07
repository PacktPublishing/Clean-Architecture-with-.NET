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

        builder.HasData(
            new Product
            {
                Id = new Guid("f0ae6d4f-a312-4a81-830d-ea0c16131326"),
                Name = "Wireless Mouse",
                Price = 29.99m,
                StockLevel = 150,
                ImageUrl = "https://placehold.co/200?text=Wireless+Mouse"
            },
            new Product
            {
                Id = new Guid("543b49a3-2daa-46b3-8498-004322f86f41"),
                Name = "Mechanical Keyboard",
                Price = 89.99m,
                StockLevel = 75,
                ImageUrl = "https://placehold.co/200?text=Mechanical+Keyboard"
            },
            new Product
            {
                Id = new Guid("875d83af-a771-4c8b-97e7-c62f2d54b19b"),
                Name = "27-inch Monitor",
                Price = 199.99m,
                StockLevel = 45,
                ImageUrl = "https://placehold.co/200?text=27-inch+Monitor"
            },
            new Product
            {
                Id = new Guid("8e8e3c26-2f5a-4ce8-a274-cdc9c83d0b12"),
                Name = "USB-C Hub",
                Price = 49.99m,
                StockLevel = 200,
                ImageUrl = "https://placehold.co/200?text=USB-C+Hub"
            },
            new Product
            {
                Id = new Guid("ce1bb70b-8faf-4fb5-bac3-8ae488a95beb"),
                Name = "Noise Cancelling Headphones",
                Price = 129.99m,
                StockLevel = 30,
                ImageUrl = "https://placehold.co/200?text=Noise+Cancelling+Headphones"
            },
            new Product
            {
                Id = new Guid("91b4da28-d060-43ea-861a-ceb8783ef880"),
                Name = "Webcam",
                Price = 69.99m,
                StockLevel = 120,
                ImageUrl = "https://placehold.co/200?text=Webcam"
            },
            new Product
            {
                Id = new Guid("77b608ae-1ce8-4bb2-b02c-edd4185e54e5"),
                Name = "Portable SSD",
                Price = 99.99m,
                StockLevel = 80,
                ImageUrl = "https://placehold.co/200?text=Portable+SSD"
            },
            new Product
            {
                Id = new Guid("d182e085-408d-4bc7-aea5-946ca3d0dcec"),
                Name = "Gaming Chair",
                Price = 249.99m,
                StockLevel = 20,
                ImageUrl = "https://placehold.co/200?text=Gaming+Chair"
            },
            new Product
            {
                Id = new Guid("7a7d74b7-e9af-41ca-9f89-69c1e7f45c8c"),
                Name = "Smart Speaker",
                Price = 39.99m,
                StockLevel = 90,
                ImageUrl = "https://placehold.co/200?text=Smart+Speaker"
            }
        );
    }
}
