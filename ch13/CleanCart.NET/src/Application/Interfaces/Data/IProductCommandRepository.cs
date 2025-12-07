using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IProductCommandRepository : ICommandService<Product, Guid>;