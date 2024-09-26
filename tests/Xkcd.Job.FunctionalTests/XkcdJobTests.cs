using Contracts.MessageBus;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xkcd.Job.FunctionalTests.Util;
using Xkcd.Job.FunctionalTests.Util.Consumers;
using Xkcd.Job.Infrastructure;
using Xkcd.Job.Infrastructure.Repositories;

namespace Xkcd.Job.FunctionalTests;

public class XkcdJobTests(CustomWebApplicationFactory<Program, XkcdContext> factory)
    : IClassFixture<CustomWebApplicationFactory<Program, XkcdContext>>
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