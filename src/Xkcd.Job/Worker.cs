using Xkcd.Job.Service;

namespace Xkcd.Job;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var xkcdNotificationService = scope.ServiceProvider.GetRequiredService<IXkcdNotificationService>();
        xkcdNotificationService.CheckAndNotify().Wait(stoppingToken);
        return Task.CompletedTask;
    }
}