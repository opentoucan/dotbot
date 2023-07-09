using Contracts.MessageBus;
using MassTransit;
using MediatR;
using MongoDB.Driver;
using Quartz;
using Xkcd.API.Commands;
using Xkcd.API.Infrastructure;
using XkcdApi;
using XkcdService = XkcdApi.XkcdService;

namespace Xkcd.API.CronJob;

public class XkcdJob : IJob
{
    private readonly ILogger<XkcdJob> _logger;
    private readonly DbContext _dbContext;
    private readonly XkcdService.XkcdServiceClient _client;
    private readonly IMediator _mediator;

    public XkcdJob(ILogger<XkcdJob> logger, DbContext dbContext, XkcdService.XkcdServiceClient client, IMediator mediator)
    {
        _logger = logger;
        _dbContext = dbContext;
        _client = client;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Checking for new XKCD comic");

        var existingXkcd = _dbContext.XkcdLatest.AsQueryable().OrderBy(x => x.ComicNumber).FirstOrDefault();

        var latestXkcd = await _client.GetXkcdAsync(new XkcdRequest());

        if (latestXkcd == null)
        {
            _logger.LogError("Failed to get latest XKCD comic");
            return;
        }

        if (existingXkcd?.ComicNumber >= latestXkcd.Id)
        {
            _logger.LogInformation("Retrieved comic is not newer than existing comic: {}", existingXkcd.ComicNumber);
            return;
        }

        _logger.LogInformation("Current comic is {}, last checked was {}",  existingXkcd?.ComicNumber, latestXkcd.Id);

        var setXkcdCommand = new SetXkcdCommand(latestXkcd.Id, latestXkcd.ImageUrl, latestXkcd.Title, latestXkcd.AltText, latestXkcd.PublishedDate.ToDateTimeOffset());
        
        await _mediator.Send(setXkcdCommand);
    }
}