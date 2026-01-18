using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Mapping;

internal sealed class InfrastructureMappingProfile : Profile
{
    public InfrastructureMappingProfile()
    {
        // Ignore EF navigation properties globally
        ShouldMapProperty = p => !p.Name.StartsWith("Nav", StringComparison.Ordinal);

        ConfigureOrderMappings();
        ConfigureShoppingCartMappings();
        ConfigureProductMappings();
        ConfigureUserMappings();
    }

    /* =========================================================
     * ORDER
     * ========================================================= */

    private void ConfigureOrderMappings()
    {
        CreateMap<Persistence.Entities.Order, Order>()
            .ConstructUsing(CreateOrder)
            .ForMember(d => d.Items, o => o.Ignore())
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CreatedOn, o => o.MapFrom(s => s.CreatedOn));

        CreateMap<Order, Persistence.Entities.Order>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<Persistence.Entities.OrderItem, OrderItem>()
            .ConstructUsing(i =>
                new OrderItem(
                    i.ProductId,
                    i.ProductName,
                    i.ProductPrice,
                    i.Quantity));

        CreateMap<OrderItem, Persistence.Entities.OrderItem>();
    }

    private static Order CreateOrder(
        Persistence.Entities.Order src,
        ResolutionContext resolutionContext) =>
        new(
            src.UserId,
            src.Items.Select(i =>
                new OrderItem(
                    i.ProductId,
                    i.ProductName,
                    i.ProductPrice,
                    i.Quantity))
            .ToList(),
            src.TotalAmount
        );

    /* =========================================================
     * SHOPPING CART
     * ========================================================= */

    private void ConfigureShoppingCartMappings()
    {
        CreateMap<ShoppingCart, Persistence.Entities.ShoppingCart>();

        CreateMap<Persistence.Entities.ShoppingCart, ShoppingCart>()
            .ConstructUsing(src => new ShoppingCart(src.UserId))
            .ForMember(d => d.Items, o => o.Ignore())
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .AfterMap(HydrateCartItems);

        CreateMap<ShoppingCartItem, Persistence.Entities.ShoppingCartItem>().ReverseMap();
    }

    private static void HydrateCartItems(Persistence.Entities.ShoppingCart src, ShoppingCart dest)
    {
        foreach (var item in src.Items)
        {
            dest.AddItem(
                item.ProductId,
                item.ProductName,
                item.ProductPrice,
                item.Quantity);
        }
    }

    /* =========================================================
     * PRODUCT
     * ========================================================= */

    private void ConfigureProductMappings()
    {
        CreateMap<Product, Persistence.Entities.Product>().ReverseMap();
    }

    /* =========================================================
     * USER
     * ========================================================= */

    private void ConfigureUserMappings()
    {
        CreateMap<User, Persistence.Entities.User>();

        CreateMap<Persistence.Entities.User, User>()
            .ConstructUsing(CreateUser)
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Roles, o => o.Ignore());
    }

    private static User CreateUser(Persistence.Entities.User src, ResolutionContext resolutionContext) =>
        new(
            src.Username,
            src.Email,
            src.FullName,
            src.Roles.Select(Enum.Parse<UserRole>).ToList()
        );
}
