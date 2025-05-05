using Domain.Entities;
using EntityAxis.Abstractions;
using System;

namespace Application.Interfaces.Data
{
    public interface IProductCommandRepository : ICommandService<Product, Guid>;
}
