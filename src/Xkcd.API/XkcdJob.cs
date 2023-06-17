using Contracts.MessageBus;
using MassTransit;
using MongoDB.Driver;
using Quartz;
using Xkcd.API.Infrastructure;
using XkcdApi;
using XkcdService = XkcdApi.XkcdService;

namespace Xkcd.API;

public class XkcdJob : IJob
{
    private readonly ILogger<XkcdJob> _logger;
    private readonly IXkcdContext _dbContext;
    private readonly XkcdService.XkcdServiceClient _client;
    private readonly IBus _bus;

    public XkcdJob(ILogger<XkcdJob> logger, IXkcdContext dbContext, XkcdService.XkcdServiceClient client, IBus bus)
    {
        _logger = logger;
        _dbContext = dbContext;
        _client = client;
        _bus = bus;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Checking for new XKCD comic");

        var xkcdLatest = _dbContext.XkcdLatest.AsQueryable().OrderBy(x => x.ComicNumber).FirstOrDefault();

        var xkcdResponse = await _client.GetXkcdAsync(new XkcdRequest());

        if (xkcdResponse == null)
        {
            _logger.LogError("Failed to get latest XKCD comic");
            return;
        }

        if (xkcdLatest?.ComicNumber >= xkcdResponse.Id) return;

        _logger.LogInformation("Current comic is {}, last checked was {}", xkcdResponse.Id, xkcdLatest?.ComicNumber);

        xkcdLatest = new Entities.Xkcd(xkcdResponse.Id, DateTimeOffset.UtcNow);
        await _dbContext.XkcdLatest.ReplaceOneAsync(Builders<Entities.Xkcd>.Filter.Eq(x => x.ComicNumber, xkcdLatest.ComicNumber), xkcdLatest, new ReplaceOptions{IsUpsert = true});

        var xkcdPostedEvent = new XkcdPostedEvent(xkcdResponse.Id, xkcdLatest.Posted, xkcdResponse.AltText, xkcdResponse.ImageUrl, xkcdResponse.Title);
        await _bus.Publish(xkcdPostedEvent, context.CancellationToken);
        /*
         
         
        foreach (var service in _senderServices)
        {
            var sendNewComicResult = await service.SendNewComic(comic);
            if (sendNewComicResult.IsFailed)
            {
                var errs = sendNewComicResult.Errors.Select(x => x.Message).ToList();
                _logger.LogError("{} failed to send latest XKCD comic: {}", service.GetType().Name,
                    string.Join(", ", errs));
            }
        }
        */
    }
}