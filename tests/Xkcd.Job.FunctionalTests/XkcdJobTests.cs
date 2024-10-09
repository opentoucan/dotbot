using Contracts.MessageBus;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Repositories;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xkcd.Job.FunctionalTests.Util;
using Xkcd.Job.FunctionalTests.Util.Consumers;

namespace Xkcd.Job.FunctionalTests;

public class XkcdJobTests(CustomWebApplicationFactory<Program, DotbotContext> factory)
    : IClassFixture<CustomWebApplicationFactory<Program, DotbotContext>>
{

    [Fact]
    public async Task NewComic_PublishesNewXkcd_WhenNoneExistInDatabase()
    {
        using var scope = factory.Services.CreateScope();
        var testHarness = factory.Services.GetTestHarness();
        var consumerHarness = testHarness.GetConsumerHarness<XkcdPostedConsumer>();
        var repository = scope.ServiceProvider.GetRequiredService<IXkcdRepository>();
        var xkcdInserted = repository.FindLatest();
        
        Assert.NotNull(xkcdInserted);
        Assert.Equal(1, consumerHarness.Consumed.Count());
        Assert.True(await consumerHarness.Consumed.Any<XkcdPostedEvent>(x => x.Context.Message.ComicNumber == xkcdInserted?.ComicNumber));
    }
}