using Contracts.MessageBus;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xkcd.Job.FunctionalTests.Util;
using Xkcd.Job.FunctionalTests.Util.Consumers;
using Xkcd.Job.Infrastructure.Repositories;

namespace Xkcd.Job.FunctionalTests;

public class XkcdJobTests : IClassFixture<XkcdJobFixture>
{
    private readonly XkcdJobFixture _fixture;
    public XkcdJobTests(XkcdJobFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NewComic_PublishesNewXkcd_WhenNoneExistInDatabase()
    {
        using var scope = _fixture.Services.CreateScope();
        var testHarness = _fixture.Services.GetTestHarness();
        var consumerHarness = testHarness.GetConsumerHarness<XkcdPostedConsumer>();
        var repository = scope.ServiceProvider.GetRequiredService<IXkcdRepository>();
        var xkcdInserted = repository.FindLatest();
        
        Assert.NotNull(xkcdInserted);
        Assert.Equal(1, consumerHarness.Consumed.Count());
        Assert.True(await consumerHarness.Consumed.Any<XkcdPostedEvent>(x => x.Context.Message.ComicNumber == xkcdInserted?.ComicNumber));
    }
}