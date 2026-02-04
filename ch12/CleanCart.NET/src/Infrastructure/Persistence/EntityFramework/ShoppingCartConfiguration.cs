using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityFramework;

internal class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.HasKey(shoppingCart => shoppingCart.Id);
        builder.Property(shoppingCart => shoppingCart.Id).ValueGeneratedNever();
        builder.HasOne(shoppingCart => shoppingCart.NavUser)
            .WithOne(user => user.NavShoppingCart)
            .HasForeignKey<ShoppingCart>(shoppingCart => shoppingCart.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ShoppingCartItem owned entity configuration
        builder.OwnsMany(e => e.Items, shoppingCartItemBuilder =>
        {
            shoppingCartItemBuilder.WithOwner(cartItem => cartItem.NavShoppingCart);
            shoppingCartItemBuilder.HasOne(cartItem => cartItem.NavProduct)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            shoppingCartItemBuilder.Property(cartItem => cartItem.Quantity).IsRequired();
            shoppingCartItemBuilder.Property(cartItem => cartItem.ProductName).IsRequired().HasMaxLength(255);
            shoppingCartItemBuilder.Property(cartItem => cartItem.ProductPrice).IsRequired().HasPrecision(18, 2);
        });
    }
}