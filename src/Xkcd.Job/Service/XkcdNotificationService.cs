using Contracts.MessageBus;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xkcd.Job.Infrastructure;
using Xkcd.Sdk;

namespace Xkcd.Job.Service;

public class XkcdNotificationService(
    DbContext dbContext,
    XkcdService client,
    IPublishEndpoint bus, 
    ILogger<XkcdNotificationService> logger)
    : IXkcdNotificationService
{
    public async Task CheckAndNotify()
    {
        logger.LogInformation("Checking for new XKCD comic");
        
        var existingXkcd = dbContext.XkcdLatest.AsQueryable().OrderBy(x => x.ComicNumber).FirstOrDefault();
        var latestXkcd = await client.GetXkcdComicAsync(null, CancellationToken.None);
        if (latestXkcd == null)
        {
            logger.LogError("Failed to get latest XKCD comic");
            return;
        }

        if (existingXkcd?.ComicNumber >= latestXkcd.ComicNumber)
        {
            logger.LogInformation("Retrieved comic is not newer than existing comic: {comicNumber}", existingXkcd.ComicNumber);
            return;
        }

        logger.LogInformation("Current comic is {existingComicNumber}, last checked was {latestComicNumber}",  existingXkcd?.ComicNumber, latestXkcd.ComicNumber);

        await dbContext.BeginTransactionAsync();
        
        var newXkcd = new Xkcd.Job.Infrastructure.Entities.Xkcd(latestXkcd.ComicNumber, latestXkcd.DatePosted);
        if (existingXkcd == null)
        {
            await dbContext.XkcdLatest.InsertOneAsync(dbContext.Session, newXkcd, cancellationToken: CancellationToken.None);
        }
        else
        {
            newXkcd.Id = existingXkcd.Id;
            await dbContext.XkcdLatest.ReplaceOneAsync(dbContext.Session,
                Builders<Xkcd.Job.Infrastructure.Entities.Xkcd>.Filter.Eq(x => x.ComicNumber, existingXkcd.ComicNumber),
                newXkcd, cancellationToken: CancellationToken.None);
        }
        
        var xkcdPostedEvent = new XkcdPostedEvent(latestXkcd.ComicNumber, latestXkcd.DatePosted, latestXkcd.AltText, latestXkcd.ImageUrl, latestXkcd.Title);
        await bus.Publish(xkcdPostedEvent, CancellationToken.None);
        await dbContext.CommitTransactionAsync();
    }
}