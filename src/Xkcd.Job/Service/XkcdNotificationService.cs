using Contracts.MessageBus;
using Dotbot.Infrastructure.Repositories;
using MassTransit;
using MediatR;
using Xkcd.Sdk;

namespace Xkcd.Job.Service;

public record XkcdCheckNewXkcdRequest : IRequest<bool>;

public class XkcdNotificationService : IRequestHandler<XkcdCheckNewXkcdRequest, bool>
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

    public async Task<bool> Handle(XkcdCheckNewXkcdRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for new XKCD comic");
        var lastPostedXkcd = _xkcdRepository.FindLatest();
        var latestXkcd = await _client.GetXkcdComicAsync(cancellationToken: cancellationToken);
        if (latestXkcd == null)
        {
            _logger.LogError("Failed to get latest XKCD comic");
            return false;
        }

        if (lastPostedXkcd?.ComicNumber >= latestXkcd.ComicNumber)
        {
            _logger.LogInformation("Retrieved comic is not newer than existing comic: {comicNumber}", lastPostedXkcd.ComicNumber);
            return false;
        }

        _logger.LogInformation("Current comic is {existingComicNumber}, last checked was {latestComicNumber}",  lastPostedXkcd?.ComicNumber, latestXkcd.ComicNumber);

        var newXkcd = new Dotbot.Infrastructure.Entities.Xkcd(latestXkcd.ComicNumber, latestXkcd.DatePosted);
        _xkcdRepository.Add(newXkcd);
        var xkcdPostedEvent = new XkcdPostedEvent(latestXkcd.ComicNumber, latestXkcd.DatePosted, latestXkcd.AltText, latestXkcd.ImageUrl, latestXkcd.Title);
        await _bus.Publish(xkcdPostedEvent, cancellationToken);
        return true;
    }
}