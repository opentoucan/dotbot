using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Xkcd.Job.Infrastructure;

public class DbContext
{
    private readonly MongoDbContext _mongoDbContext;
    private CancellationTokenSource _cancellationTokenSource = null!;
    
    public IClientSessionHandle? Session { get; private set; }
    public Guid? TransactionId { get; private set; }
    public IMongoCollection<Entities.Xkcd> XkcdLatest { get; }
    
    public DbContext(MongoDbContext mongoDbContext, IMongoCollection<Entities.Xkcd> xkcdLatest)
    {
        _mongoDbContext = mongoDbContext;
        XkcdLatest = xkcdLatest;
    }

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