using System.Net;
using System.Net.Http.Json;
using Dotbot.Api.Discord.SlashCommandApis;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NSubstitute;
using RichardSzalay.MockHttp;
using ServiceDefaults;

namespace Dotbot.Api.UnitTests.Discord.SlashCommandApiTests;
public class XkcdSlashCommandsTests
{
    private static readonly Instrumentation Instrumentation = new();
    private static readonly IRestRequestHandler RestRequestHandlerMock = Substitute.For<IRestRequestHandler>();
#pragma warning disable TUnit0023 // Member should be disposed within a clean up method
    private static readonly HttpApplicationCommandContext CommandContext = new(new SlashCommandInteraction(new JsonInteraction
#pragma warning restore TUnit0023 // Member should be disposed within a clean up method
    {
        Data = new JsonInteractionData(),
        User = new JsonUser(),
        GuildUser = new JsonGuildUser(),
        Channel = new JsonChannel(),
        Entitlements =
                    []
    }, new Guild(new JsonGuild(), 1234, new RestClient(new RestClientConfiguration()), IDictionaryProvider.OfDictionary),
            (_, _, _, _, _) => Task.FromResult<InteractionCallbackResponse?>(new InteractionCallbackResponse(new NetCord.Rest.JsonModels.JsonInteractionCallbackResponse(), new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock }))), new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock })),
        new RestClient(new RestClientConfiguration()));

    private static readonly ILoggerFactory LoggerFactory = Substitute.For<ILoggerFactory>();
    private readonly MockHttpMessageHandler _handler = new();
    private readonly string _baseAddress = "https://www.example.com";


    [Test]
    public async Task XkcdCommand_XkcdDoesNotExist_ReturnsNotFoundErrorMsg()
    {
        var comicNumber = 99;
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        IXkcdService xkcdService = new XkcdService(httpClient, Substitute.For<ILogger<XkcdService>>());

        _handler
            .Expect(HttpMethod.Get, $"{_baseAddress}/{comicNumber}/info.0.json")
            .Respond(HttpStatusCode.NotFound, JsonContent.Create(""));


        var sut = await XkcdSlashCommands.FetchXkcdAsync(xkcdService, Instrumentation, LoggerFactory, CommandContext, comicNumber);
        await Assert.That(sut.Content).IsEqualTo($"XKCD comic #{comicNumber} does not exist");
    }

    [Test]
    public async Task XkcdCommand_XkcdCannotBeRetrieved_ReturnsGenericErrorMsg()
    {

        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        IXkcdService xkcdService = new XkcdService(httpClient, Substitute.For<ILogger<XkcdService>>());

        _handler
            .Expect(HttpMethod.Get, $"{_baseAddress}/info.0.json")
            .Respond(HttpStatusCode.ServiceUnavailable, JsonContent.Create(""));


        var sut = await XkcdSlashCommands.FetchXkcdAsync(xkcdService, Instrumentation, LoggerFactory, CommandContext);
        await Assert.That(sut.Content).IsEqualTo("There was an issue fetching the XKCD");
    }

    [Test]
    public async Task XkcdCommand_XkcdIsRetrieved_ReturnsXkcd()
    {
        var comicNumber = 1;

        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        IXkcdService xkcdService = new XkcdService(httpClient, Substitute.For<ILogger<XkcdService>>());

        _handler
            .Expect(HttpMethod.Get, $"{_baseAddress}/{comicNumber}/info.0.json")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new XkcdService.XkcdContent { Title = "Comic #1", Num = comicNumber, Alt = "Comic #1", Img = "some_url", Year = 2024, Month = 1, Day = 1 }));

        var sut = await XkcdSlashCommands.FetchXkcdAsync(xkcdService, Instrumentation, LoggerFactory, CommandContext, comicNumber);

        await Assert.That(sut.Embeds?.Count()).IsEqualTo(1);
        await Assert.That(sut.Embeds?.First().Title).Contains($"#{comicNumber}");
    }

    [Test]
    public async Task XkcdCommand_XkcdComicNumberIsNotProvided_ReturnsLatestXkcd()
    {
        var comicNumber = 999;

        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        IXkcdService xkcdService = new XkcdService(httpClient, Substitute.For<ILogger<XkcdService>>());

        _handler
            .Expect(HttpMethod.Get, $"{_baseAddress}/info.0.json")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new XkcdService.XkcdContent { Title = $"Comic #{comicNumber}", Num = comicNumber, Alt = "Comic #1", Img = "some_url", Year = 2024, Month = 1, Day = 1 }));

        var sut = await XkcdSlashCommands.FetchXkcdAsync(xkcdService, Instrumentation, LoggerFactory, CommandContext);

        await Assert.That(sut.Embeds?.Count()).IsEqualTo(1);
        await Assert.That(sut.Embeds?.First().Title).Contains($"Latest comic #{comicNumber}");
    }
}