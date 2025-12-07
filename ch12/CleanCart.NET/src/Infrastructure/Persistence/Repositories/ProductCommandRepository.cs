using Application.Interfaces.Data;
using AutoMapper;
using Domain.Entities;
using EntityAxis.EntityFramework;
using EntityAxis.KeyMappers;
using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductCommandRepository(IDbContextFactory<CoreDbContext> contextFactory, IMapper mapper, IKeyMapper<Guid, Guid> keyMapper)
    : EntityFrameworkCommandService<Product, Entities.Product, CoreDbContext, Guid, Guid>(contextFactory, mapper, keyMapper), IProductCommandRepository;