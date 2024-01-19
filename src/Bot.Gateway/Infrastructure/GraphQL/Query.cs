using Bot.Gateway.Infrastructure.Entities;
using HotChocolate.Data;
using MongoDB.Driver;

namespace Bot.Gateway.Infrastructure.GraphQL;

public class Query
{
    [UsePaging]
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IExecutable<BotCommand> SearchBotCommands([Service] IMongoCollection<BotCommand> botCommands) => botCommands.AsExecutable();
    
    [UsePaging]
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IExecutable<DiscordServer> SearchDiscordServers([Service] IMongoCollection<DiscordServer> discordServers) => discordServers.AsExecutable();
}