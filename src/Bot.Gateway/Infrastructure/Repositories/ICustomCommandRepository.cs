using Bot.Gateway.Infrastructure.Entities;

namespace Bot.Gateway.Infrastructure.Repositories;

public interface ICustomCommandRepository : IRepository<CustomCommand>
{
    Task<CustomCommand?> GetByNameAsync(string name);
    CustomCommand Add(CustomCommand command);
    void Update(CustomCommand command);
}