using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IOrderCommandRepository : ICommandService<Order, Guid>
{
    Task<Order> CreateOrderAsync(Order order);
}