using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotbot.Api.Application.Queries;

public class GuildQueries(DotbotContext context) : IGuildQueries
{
    public async Task<IEnumerable<CustomCommand>> GetAllCustomCommands(string externalId)
    {
        return (await context.Guilds
            .Include(g => g.CustomCommands)
            .ThenInclude(cc => cc.Attachments)
            .FirstOrDefaultAsync(g => g.ExternalId == externalId))?.CustomCommands ?? [];
    }

    public async Task<IEnumerable<CustomCommand>> GetCustomCommandsByFuzzySearchOnNameAsync(string externalId, string name)
    {
        var guild = await context.Guilds
            .Include(g => g.CustomCommands)
            .FirstOrDefaultAsync(g => g.ExternalId == externalId);

        if (guild is null)
            throw new Exception("Guild not found");

        return guild.CustomCommands
            .Where(cc => cc.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))
            .Take(10)
            .ToList();
    }
}
