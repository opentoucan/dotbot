using Bot.Gateway.Infrastructure.Entities;

namespace Bot.Gateway.Application.Queries;

public interface ICustomCommandQueries
{
    Task<IEnumerable<CustomCommand>> GetCustomCommandsFromServerAsync(string externalId);
    Task<IEnumerable<CustomCommand>> GetCustomCommandsByFuzzySearchOnNameAsync(string name);
}