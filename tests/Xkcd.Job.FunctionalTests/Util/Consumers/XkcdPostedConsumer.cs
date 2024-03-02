using Contracts.MessageBus;
using MassTransit;

namespace Xkcd.Job.FunctionalTests.Util.Consumers;

public class XkcdPostedConsumer
    : IConsumer<XkcdPostedEvent>
{
    public Task Consume(ConsumeContext<XkcdPostedEvent> context)
    {
        return Task.CompletedTask;
    }
}