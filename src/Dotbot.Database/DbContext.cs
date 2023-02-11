using Dotbot.Database.Entities;
using MongoDB.Driver;

namespace Dotbot.Database;

public class DbContext
{
    public DbContext(IMongoCollection<BotCommand> botCommands, IMongoCollection<ChatServer> chatServers, IMongoCollection<PersistentSetting> persistentSettings)
    {
        BotCommands = botCommands;
        ChatServers = chatServers;
        PersistentSettings = persistentSettings;
    }

    public IMongoCollection<BotCommand> BotCommands { get; }
    public IMongoCollection<ChatServer> ChatServers { get; }
    public IMongoCollection<PersistentSetting> PersistentSettings { get; }
}