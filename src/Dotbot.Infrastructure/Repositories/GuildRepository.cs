using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Dotbot.Infrastructure.Repositories;

public class GuildRepository(DotbotContext context) : IGuildRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Guild?> GetByExternalIdAsync(string externalId)
    {
        var guild = await context.Guilds.FirstOrDefaultAsync(x => x.ExternalId == externalId);

        if (guild is not null)
        {
            await context.Entry(guild)
                .Collection(g => g.CustomCommands)
                .Query()
                .Include(cc => cc.Attachments)
                .LoadAsync();
        }

        return guild;
    }

    public Guild Add(Guild guild)
    {
        return context.Guilds.Add(guild).Entity;
    }

    public void Update(Guild guild)
    {
        context.Entry(guild).State = EntityState.Modified;
    }

    public void Remove(Guild guild)
    {
        context.Entry(guild).State = EntityState.Deleted;
    }
}