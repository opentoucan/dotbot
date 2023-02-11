using Dotbot.Common.Services;
using Dotbot.Common.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot;

public class XkcdHostedService : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IEnumerable<IXkcdSenderService> _senderServices;
    private readonly IXkcdService _xkcdService;
    private readonly ILogger<XkcdHostedService> _logger;
    private readonly BotSettings _botSettings;

    public XkcdHostedService(IXkcdService xkcdService, IOptions<BotSettings> botSettings,
        ILogger<XkcdHostedService> logger, IEnumerable<IXkcdSenderService> senderServices)
    {
        _xkcdService = xkcdService;
        _logger = logger;
        _senderServices = senderServices;
        _botSettings = botSettings.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running");

        _timer = new Timer(CheckXkcd, null, TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(_botSettings.XkcdComicCheckIntervalMinutes));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void CheckXkcd(object? state)
    {
        _logger.LogInformation("Checking for new XKCD comic");

        var result = await _xkcdService.GetLast();
        var lastNumber = 0;
        if (result.IsSuccess)
        {
            lastNumber = result.Value;
        }

        var (_, isFailed, comic, errors) = await _xkcdService.GetLatestComic();

        if (isFailed)
        {
            _logger.LogError("Failed to get latest XKCD comic {}", string.Join(",", errors.Select(x => x.Message)));
            return;
        }

        if (lastNumber >= comic.Num) return;

        _logger.LogInformation("Current comic is {}, last checked was {}", comic.Num, lastNumber);

        await _xkcdService.SetLast(comic.Num);

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
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}