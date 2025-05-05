using Domain.Entities;
using EntityAxis.Abstractions;
using System;

namespace Application.Interfaces.Data
{
    public interface IProductQueryRepository : IQueryService<Product, Guid>;
}
