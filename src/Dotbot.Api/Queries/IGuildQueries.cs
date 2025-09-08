using Dotbot.Infrastructure.Entities;

namespace Dotbot.Api.Queries;

public interface IGuildQueries
{
    Task<IEnumerable<CustomCommand>> GetAllCustomCommands(string externalId);
    Task<IEnumerable<CustomCommand>> GetCustomCommandsByFuzzySearchOnNameAsync(string externalId, string name);
}