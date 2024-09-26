using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.SeedWork;

namespace Bot.Gateway.Infrastructure.Repositories;

public interface IRepository<T> where T : Entity
{
    IUnitOfWork UnitOfWork { get; }
}