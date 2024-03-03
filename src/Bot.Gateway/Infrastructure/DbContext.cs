using Bot.Gateway.Infrastructure.Entities;
using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Bot.Gateway.Infrastructure;

public class DbContext
{
    private readonly MongoDbContext _mongoDbContext;
   
    public IClientSessionHandle? Session { get; private set; }
    
    public DbContext(MongoDbContext mongoDbContext, IMongoCollection<BotCommand> botCommands)
    {
        _mongoDbContext = mongoDbContext;
        BotCommands = botCommands;
    }

    public IMongoCollection<BotCommand> BotCommands { get; }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        await _mongoDbContext.BeginTransaction(cancellationToken);
        Session = _mongoDbContext.Session;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _mongoDbContext.CommitTransaction(cancellationToken);
            Session = null;
        }
        catch
        {
            RollbackTransaction(cancellationToken);
            Session = null;
            throw;
        }
    }

    public void RollbackTransaction(CancellationToken cancellationToken)
    {
        _mongoDbContext.AbortTransaction(cancellationToken);
        Session = null;
    }
}