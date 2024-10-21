using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Persistence.Repositories;

// https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/primary-constructors
public abstract class RepositoryBase<TDbContext>(IDbContextFactory<TDbContext> contextFactory, IMapper mapper) where TDbContext : DbContext
{
    protected IDbContextFactory<TDbContext> ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    protected IMapper Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
}