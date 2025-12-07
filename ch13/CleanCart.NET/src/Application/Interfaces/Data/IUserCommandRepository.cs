using Domain.Entities;
using EntityAxis.Abstractions;

namespace Application.Interfaces.Data;

public interface IUserCommandRepository : ICommandService<User, Guid>;