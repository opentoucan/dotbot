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

    public Task Upsert(Entities.Xkcd xkcd)
    {
        _dbContext.XkcdLatest.ReplaceOne(
            session: _dbContext.Session,
            filter: Builders<Entities.Xkcd>.Filter.Eq(x => x.ComicNumber, xkcd.ComicNumber),
            replacement: xkcd,
            options: new ReplaceOptions { IsUpsert = true });
        return Task.CompletedTask;
    }
}