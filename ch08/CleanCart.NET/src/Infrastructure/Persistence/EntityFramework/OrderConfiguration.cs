using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityFramework;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.HasOne(order => order.NavUser)
            .WithMany(user => user.NavOrders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(order => order.CreatedOn).IsRequired();
        builder.Property(order => order.Status).IsRequired().HasMaxLength(20).HasConversion<string>();

        // OrderItem owned entity configuration
        builder.OwnsMany(order => order.Items, orderItemBuilder =>
        {
            orderItemBuilder.WithOwner(orderItem => orderItem.NavOrder);
            orderItemBuilder.HasOne(orderItem => orderItem.NavProduct)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            orderItemBuilder.Property(orderItem => orderItem.Quantity).IsRequired();
            orderItemBuilder.Property(orderItem => orderItem.ProductName).IsRequired().HasMaxLength(255);
            orderItemBuilder.Property(orderItem => orderItem.ProductPrice).IsRequired().HasPrecision(18, 2);
        });
    }
}