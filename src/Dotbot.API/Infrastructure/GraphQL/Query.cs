using Dotbot.Models;
using HotChocolate.Data;
using MongoDB.Driver;

namespace Dotbot.Infrastructure.GraphQL;

public class Query
{
    [UsePaging]
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IExecutable<BotCommand> SearchBotCommands([Service] IMongoCollection<BotCommand> botCommands) =>
        botCommands.AsExecutable();
}