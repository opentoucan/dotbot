using MediatR;
using Xkcd.Job.Service;

namespace Xkcd.Job;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public Worker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        mediator.Send(new XkcdCheckNewXkcdRequest(), stoppingToken).Wait(stoppingToken);
        return Task.CompletedTask;
    }
}