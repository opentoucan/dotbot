using Discord.Entities;
using MongoDB.Driver;

namespace Discord;

public class DbContext
{
    public DbContext( IMongoCollection<DiscordServer> discordServers)
    {
        DiscordServers = discordServers;
    }

    public IMongoCollection<DiscordServer> DiscordServers { get; }
}