using MongoDB.Driver;

namespace Xkcd.Job.Infrastructure.Repositories;

public interface IXkcdRepository
{
    IUnitOfWork UnitOfWork { get; }
    Entities.Xkcd? FindLatest();
    Task Upsert(Entities.Xkcd xkcd);
}

public class XkcdRepository : IXkcdRepository
{
    private readonly XkcdContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public XkcdRepository(XkcdContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Entities.Xkcd? FindLatest()
    {
        return _dbContext.XkcdLatest.AsQueryable().OrderByDescending(x => x.ComicNumber).FirstOrDefault();
    }

    public async Task Upsert(Entities.Xkcd xkcd)
    {
        var updateDefinition = Builders<Entities.Xkcd>.Update.Combine(
            Builders<Entities.Xkcd>.Update.Set(x => x.ComicNumber, xkcd.ComicNumber),
            Builders<Entities.Xkcd>.Update.Set(x => x.Posted, xkcd.Posted));
        
        var replaced = await _dbContext.XkcdLatest.FindOneAndUpdateAsync(
            session: _dbContext.Session,
            filter: Builders<Entities.Xkcd>.Filter.Lte(x => x.ComicNumber, xkcd.ComicNumber),
            update: updateDefinition);
        if (replaced is null)
            await _dbContext.XkcdLatest.InsertOneAsync(
                session: _dbContext.Session,
                document: xkcd);
    }
}