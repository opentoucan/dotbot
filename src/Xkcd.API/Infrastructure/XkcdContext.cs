using MassTransit.MongoDbIntegration;
using MongoDB.Driver;

namespace Xkcd.API.Infrastructure;

public interface IXkcdContext
{
    IMongoCollection<Entities.Xkcd> XkcdLatest { get; }
}

public class XkcdContext : IXkcdContext
{
        
    private readonly MongoDbContext _mongoDbContext;
    public IMongoCollection<Entities.Xkcd> XkcdLatest { get; }
    
    public XkcdContext(MongoDbContext mongoDbContext, IMongoCollection<Entities.Xkcd> xkcdLatest)
    {
        _mongoDbContext = mongoDbContext;
        XkcdLatest = xkcdLatest;
    }
}