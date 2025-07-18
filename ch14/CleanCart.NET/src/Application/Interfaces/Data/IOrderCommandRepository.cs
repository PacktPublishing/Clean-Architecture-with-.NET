using System;
using Domain.Entities;
using System.Threading.Tasks;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data
{
    public interface IOrderCommandRepository : ICommandService<Order, Guid>
    {
        Task<Order> CreateOrderAsync(Order order);
    }
}