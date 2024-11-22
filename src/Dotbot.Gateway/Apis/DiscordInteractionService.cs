using System.Diagnostics;
using System.Diagnostics.Metrics;
using Dotbot.Gateway.Application.Queries;
using MassTransit;
using MassTransit.Scheduling;

namespace Dotbot.Gateway.Apis;

public class DiscordInteractionService(
    IBus bus,
    IGuildQueries queries,
    IPublishEndpoint publishEndpoint,
    ILogger<DiscordInteractionService> logger,
    IMeterFactory meterFactory)
{
    public IBus Bus { get; set; } = bus;
    public MessageScheduler Scheduler { get; set; } = new(new DelayedScheduleMessageProvider(bus), bus.Topology as IRabbitMqBusTopology);
    public IPublishEndpoint PublishEndpoint { get; set; } = publishEndpoint; 
    public IGuildQueries Queries { get; set; } = queries;
    public ILogger<DiscordInteractionService> Logger { get; set; } = logger;
    public IMeterFactory MeterFactory { get; } = meterFactory;
    public static ActivitySource ActivitySource { get; } = new(nameof(DiscordInteractionApi));
}