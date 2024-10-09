using Dotbot.Infrastructure.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Dotbot.Infrastructure.Repositories;

public interface IXkcdRepository
{
    IUnitOfWork UnitOfWork { get; }
    Entities.Xkcd? FindLatest();
    Entities.Xkcd Add(Entities.Xkcd xkcd);
    void Update(Entities.Xkcd xkcd);
}

public class XkcdRepository : IXkcdRepository
{
    private readonly DotbotContext _dbContext;
    public IUnitOfWork UnitOfWork => _dbContext;

    public XkcdRepository(DotbotContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Entities.Xkcd? FindLatest()
    {
        return _dbContext.Xkcds.OrderByDescending(x => x.ComicNumber).FirstOrDefault();
    }

    public Entities.Xkcd Add(Entities.Xkcd xkcd)
    {
        return _dbContext.Xkcds.Add(xkcd).Entity;
    }

    public void Update(Entities.Xkcd xkcd)
    {
        _dbContext.Entry(xkcd).State = EntityState.Modified;
    }
}