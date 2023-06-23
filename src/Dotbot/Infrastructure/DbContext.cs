using Dotbot.Models;
using MongoDB.Driver;

namespace Dotbot.Infrastructure;

public class DbContext
{
    public DbContext(IMongoCollection<BotCommand> botCommands)
    {
        BotCommands = botCommands;
    }

    public IMongoCollection<BotCommand> BotCommands { get; }
}