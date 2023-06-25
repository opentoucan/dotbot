using Dotbot.Models;
using HotChocolate.Data;
using MongoDB.Driver;

namespace Dotbot.Infrastructure.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IExecutable<BotCommand> GetBotCommands([Service] IMongoCollection<BotCommand> botCommands) =>
        botCommands.AsExecutable();
}