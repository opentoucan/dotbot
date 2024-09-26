using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bot.Gateway.Application.Queries;

public class CustomCommandQueries(DotbotContext context) : ICustomCommandQueries
{
    public async Task<IEnumerable<CustomCommand>> GetCustomCommandsFromServerAsync(string externalId)
    {
        return await context.CustomCommands
            .Where(c => c.Guild.ExternalId == externalId)
            .Include(c => c.Attachments)
            .ToListAsync();
    }

    public async Task<IEnumerable<CustomCommand>> GetCustomCommandsByFuzzySearchOnNameAsync(string name)
    {
        return await context.CustomCommands
            .Where(x => EF.Functions.Like(x.Name, $"%{name}%"))
            .Take(10)
            .ToListAsync();
    }
}
