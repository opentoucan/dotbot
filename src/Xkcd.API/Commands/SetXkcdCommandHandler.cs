using Contracts.MessageBus;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using Xkcd.API.Infrastructure;

namespace Xkcd.API.Commands;

public class SetXkcdCommandHandler : IRequestHandler<SetXkcdCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly IBus _bus;
    public SetXkcdCommandHandler(DbContext dbContext, IBus bus)
    {
        _dbContext = dbContext;
        _bus = bus;
    }

    public async Task<bool> Handle(SetXkcdCommand request, CancellationToken cancellationToken)
    {
        var newXkcd = new Infrastructure.Entities.Xkcd(request.ComicNumber, request.DatePosted);
        var existingXkcd = _dbContext.XkcdLatest.AsQueryable().OrderBy(x => x.ComicNumber).FirstOrDefault();
        if (existingXkcd == null)
        {
            await _dbContext.XkcdLatest.InsertOneAsync(_dbContext.Session, newXkcd, cancellationToken: cancellationToken);
        }
        else
        {

            newXkcd.Id = existingXkcd.Id;
            await _dbContext.XkcdLatest.ReplaceOneAsync(_dbContext.Session,
                Builders<Infrastructure.Entities.Xkcd>.Filter.Eq(x => x.ComicNumber, existingXkcd.ComicNumber),
                newXkcd, cancellationToken: cancellationToken);
        }

        
        var xkcdPostedEvent = new XkcdPostedEvent(request.ComicNumber, request.DatePosted, request.AltText, request.ImageUrl, request.Title);
        await _bus.Publish(xkcdPostedEvent, cancellationToken);
        return true;
    }
}