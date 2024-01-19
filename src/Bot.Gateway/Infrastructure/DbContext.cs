using Bot.Gateway.Infrastructure.Entities;
using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Bot.Gateway.Infrastructure;

public class DbContext
{
    private readonly MongoDbContext _mongoDbContext;
    private CancellationTokenSource _cancellationTokenSource = null!;
    
    public IClientSessionHandle? Session { get; private set; }
    public Guid? TransactionId { get; private set; }
    
    public DbContext(MongoDbContext mongoDbContext, IMongoCollection<BotCommand> botCommands, IMongoCollection<DiscordServer> discordServers)
    {
        _mongoDbContext = mongoDbContext;
        BotCommands = botCommands;
        DiscordServers = discordServers;
    }

    public IMongoCollection<BotCommand> BotCommands { get; }
    public IMongoCollection<DiscordServer> DiscordServers { get; }

    public async Task BeginTransactionAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await _mongoDbContext.BeginTransaction(_cancellationTokenSource.Token);
        TransactionId = _mongoDbContext.TransactionId;
        Session = _mongoDbContext.Session;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _mongoDbContext.CommitTransaction(_cancellationTokenSource.Token);
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            TransactionId = null;
            Session = null;
        }
    }

    public void RollbackTransaction()
    {
        _mongoDbContext.AbortTransaction(_cancellationTokenSource.Token);
    }
}