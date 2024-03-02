using Contracts.MessageBus;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xkcd.Job.Infrastructure;
using Xkcd.Job.Infrastructure.Repositories;
using Xkcd.Sdk;

namespace Xkcd.Job.Service;

public class XkcdNotificationService : IXkcdNotificationService
{
    private readonly IXkcdRepository _xkcdRepository;
    private readonly IXkcdService _client;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<XkcdNotificationService> _logger;

    public XkcdNotificationService(
        IXkcdRepository xkcdRepository,
        IXkcdService client,
        IPublishEndpoint bus, 
        ILogger<XkcdNotificationService> logger)
    {
        _xkcdRepository = xkcdRepository;
        _client = client;
        _bus = bus;
        _logger = logger;
    }

    public async Task CheckAndNotify()
    {
        _logger.LogInformation("Checking for new XKCD comic");
        var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var cancellationToken = cancellationSource.Token;
        var existingXkcd = _xkcdRepository.FindLatest();
        var latestXkcd =  await _client.GetXkcdComicAsync(cancellationToken: cancellationToken);
        if (latestXkcd == null)
        {
            _logger.LogError("Failed to get latest XKCD comic");
            return;
        }

        if (existingXkcd?.ComicNumber >= latestXkcd.ComicNumber)
        {
            _logger.LogInformation("Retrieved comic is not newer than existing comic: {comicNumber}", existingXkcd.ComicNumber);
            return;
        }

        _logger.LogInformation("Current comic is {existingComicNumber}, last checked was {latestComicNumber}",  existingXkcd?.ComicNumber, latestXkcd.ComicNumber);

        await _xkcdRepository.UnitOfWork.BeginTransactionAsync(cancellationToken);
        
        var newXkcd = new Xkcd.Job.Infrastructure.Entities.Xkcd(latestXkcd.ComicNumber, latestXkcd.DatePosted);
        try
        {
            await _xkcdRepository.Upsert(newXkcd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        var xkcdPostedEvent = new XkcdPostedEvent(latestXkcd.ComicNumber, latestXkcd.DatePosted, latestXkcd.AltText, latestXkcd.ImageUrl, latestXkcd.Title);
        await _bus.Publish(xkcdPostedEvent, cancellationToken);
        await _xkcdRepository.UnitOfWork.CommitTransactionAsync(cancellationToken);
    }
}