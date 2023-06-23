using Dotbot.Discord.Entities;
using MongoDB.Driver;

namespace Dotbot.Discord;

public class DbContext
{
    public DbContext( IMongoCollection<DiscordServer> discordServers)
    {
        DiscordServers = discordServers;
    }

    public IMongoCollection<DiscordServer> DiscordServers { get; }
}