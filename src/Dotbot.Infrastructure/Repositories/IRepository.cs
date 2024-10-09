using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.SeedWork;

namespace Dotbot.Infrastructure.Repositories;

public interface IRepository<T> where T : Entity
{
    IUnitOfWork UnitOfWork { get; }
}