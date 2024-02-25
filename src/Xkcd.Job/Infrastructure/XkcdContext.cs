using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Xkcd.Job.Infrastructure;

public class XkcdContext : IUnitOfWork
{
    private readonly MongoDbContext _mongoDbContext;
    
    public IClientSessionHandle? Session { get; private set; }
    public IMongoCollection<Entities.Xkcd> XkcdLatest { get; }
    
    public XkcdContext(MongoDbContext mongoDbContext, IMongoCollection<Entities.Xkcd> xkcdLatest)
    {
        _mongoDbContext = mongoDbContext;
        XkcdLatest = xkcdLatest;
    }

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