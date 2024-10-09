using Contracts.MessageBus;
using Dotbot.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xkcd.Job.Service;
using Xkcd.Sdk;

namespace Xkcd.Job.UnitTests.Service;

public class XkcdNotificationServiceTests
{
    public IXkcdRepository Repository;
    public IXkcdService XkcdService;        
    public IPublishEndpoint Bus;
    public ILogger<XkcdNotificationService> Logger;

    public XkcdNotificationServiceTests()
    {
        Repository = Substitute.For<IXkcdRepository>();
        XkcdService = Substitute.For<IXkcdService>();
        Bus = Substitute.For<IPublishEndpoint>();
        Logger = Substitute.For<ILogger<XkcdNotificationService>>();
    }
    
    [Fact]
    public async Task CheckAndNotify_XkcdApiError_DoesNotSaveOrPublish()
    {
        var sut = new XkcdNotificationService(Repository, XkcdService, Bus, Logger);
        
        await sut.Handle(new XkcdCheckNewXkcdRequest(), new CancellationToken());

        Repository
            .DidNotReceive()
            .Add(Arg.Any<Dotbot.Infrastructure.Entities.Xkcd>());

        await Bus
            .DidNotReceive()
            .Publish(Arg.Any<XkcdPostedEvent>(),
                Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task CheckAndNotify_OldComicFromApi_DoesNotSaveOrPublish()
    {
        var stubXkcd = new XkcdComic
        {
            ComicNumber = 99,
            Title = "Unit testing",
            AltText = "Something about testing",
            DatePosted = new DateTime(2024, 02, 24),
            ImageUrl = "https://example.com"
        };
        
        var existingXkcd =
            new Dotbot.Infrastructure.Entities.Xkcd(stubXkcd.ComicNumber, stubXkcd.DatePosted);
        XkcdService
            .GetXkcdComicAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(stubXkcd);
        Repository
            .FindLatest()
            .Returns(existingXkcd);
        
        var sut = new XkcdNotificationService(Repository, XkcdService, Bus, Logger);
        
        await sut.Handle(new XkcdCheckNewXkcdRequest(), new CancellationToken());

        Repository
            .DidNotReceive()
            .Add(Arg.Any<Dotbot.Infrastructure.Entities.Xkcd>());

        await Bus
            .DidNotReceive()
            .Publish(Arg.Any<XkcdPostedEvent>(),
                Arg.Any<CancellationToken>());
    }

    
    [Fact]
    public async Task CheckAndNotify_FirstComicPull_SavesAndPublishes()
    {
        var stubXkcd = new XkcdComic
        {
            ComicNumber = 99,
            Title = "Unit testing",
            AltText = "Something about testing",
            DatePosted = new DateTime(2024, 02, 24),
            ImageUrl = "https://example.com"
        };
        
        XkcdService
            .GetXkcdComicAsync(null, Arg.Any<CancellationToken>())
            .Returns(stubXkcd);
        
        var sut = new XkcdNotificationService(Repository, XkcdService, Bus, Logger);
        
        await sut.Handle(new XkcdCheckNewXkcdRequest(), new CancellationToken());

        Repository
            .Received(1)
            .Add(Arg.Is<Dotbot.Infrastructure.Entities.Xkcd>(x =>
                x.ComicNumber == stubXkcd.ComicNumber &&
                x.Posted.DateTime == stubXkcd.DatePosted));

        await Bus
            .Received(1)
            .Publish(Arg.Is<XkcdPostedEvent>(x =>
                x.ComicNumber == stubXkcd.ComicNumber &&
                x.DatePosted == stubXkcd.DatePosted &&
                x.ImageUrl == stubXkcd.ImageUrl &&
                x.Title == stubXkcd.Title &&
                x.AltText == stubXkcd.AltText),
                Arg.Any<CancellationToken>());
    }
}