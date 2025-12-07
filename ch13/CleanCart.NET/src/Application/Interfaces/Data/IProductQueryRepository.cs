using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IProductQueryRepository : IQueryService<Product, Guid>;