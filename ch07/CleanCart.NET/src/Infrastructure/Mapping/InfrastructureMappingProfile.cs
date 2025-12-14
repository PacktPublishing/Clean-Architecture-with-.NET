using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Mapping;

internal class InfrastructureMappingProfile : Profile
{
    public InfrastructureMappingProfile()
    {
        ShouldMapProperty = propertyInfo => !propertyInfo.Name.StartsWith("Nav", StringComparison.Ordinal);

        CreateMap<Persistence.Entities.Order, Order>()
            .ConstructUsing(src => new Order(src.UserId, src.Items.Select(itemDto => new OrderItem(itemDto.ProductId, itemDto.ProductName, itemDto.ProductPrice, itemDto.Quantity)).ToList(), src.TotalAmount))
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.Id, options => options.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedOn, options => options.MapFrom(src => src.CreatedOn))
            .ForMember(dest => dest.Status, options => options.ConvertUsing(new StringToEnumConverter<OrderStatus>(), src => src.Status));
        CreateMap<Order, Persistence.Entities.Order>()
            .ForMember(dest  => dest.Status, options => options.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, Persistence.Entities.OrderItem>();
        CreateMap<Persistence.Entities.OrderItem, OrderItem>()
            .ConstructUsing(src => new OrderItem(src.ProductId, src.ProductName, src.ProductPrice, src.Quantity));

        CreateMap<Product, Persistence.Entities.Product>();
        CreateMap<Persistence.Entities.Product, Product>();

        CreateMap<ShoppingCart, Persistence.Entities.ShoppingCart>();
        CreateMap<Persistence.Entities.ShoppingCart, ShoppingCart>()
            .ConstructUsing(src => new ShoppingCart(src.UserId))
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.Id, options => options.MapFrom(src => src.Id))
            .AfterMap((src, dest) =>
            {
                foreach (var item in src.Items)
                {
                    dest.AddItem(item.ProductId, item.ProductName, item.ProductPrice, item.Quantity);
                }
            });

        CreateMap<ShoppingCartItem, Persistence.Entities.ShoppingCartItem>();
        CreateMap<Persistence.Entities.ShoppingCartItem, ShoppingCartItem>();

        CreateMap<User, Persistence.Entities.User>();
        CreateMap<Persistence.Entities.User, User>()
            .ConstructUsing(src =>
                new User(
                    src.Username,
                    src.Email,
                    src.FullName,
                    src.Roles.Select(Enum.Parse<UserRole>).ToList()
                )
            )
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

    }

    public class StringToEnumConverter<TEnumerationType> : IValueConverter<string?, TEnumerationType> where TEnumerationType : struct, IConvertible, IComparable, IFormattable
    {
        public TEnumerationType Convert(string? sourceMember, ResolutionContext context)
        {
            Enum.TryParse(sourceMember, out TEnumerationType value);
            return value;
        }
    }
}