using Contracts.MessageBus;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xkcd.Job.FunctionalTests.Util;
using Xkcd.Job.FunctionalTests.Util.Consumers;
using Xkcd.Job.Infrastructure.Repositories;

namespace Xkcd.Job.FunctionalTests;

public class XkcdJobTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    public XkcdJobTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NewComic_PublishesNewXkcd_WhenNoneExistInDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var testHarness = scope.ServiceProvider.GetTestHarness();
        await testHarness.Start();
        var consumerHarness = testHarness.GetConsumerHarness<XkcdPostedConsumer>();
        var repository = scope.ServiceProvider.GetRequiredService<IXkcdRepository>();
        var xkcdInserted = repository.FindLatest();
        
        Assert.NotNull(xkcdInserted);
        Assert.Equal(1, consumerHarness.Consumed.Count());
        Assert.True(await consumerHarness.Consumed.Any<XkcdPostedEvent>(x => x.Context.Message.ComicNumber == xkcdInserted?.ComicNumber));
    }
}