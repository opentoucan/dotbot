using System.Net;
using System.Net.Http.Json;
using Dotbot.Api.Application.Api;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace Dotbot.Api.UnitTests.Api.SlashCommandApiTests;
public class XkcdCommandTests
{
    private static readonly HttpApplicationCommandContext CommandContext = new(new SlashCommandInteraction(new JsonInteraction
            {
                Data = new JsonInteractionData(), User = new JsonUser(), GuildUser = new JsonGuildUser(),
                Channel = new JsonChannel(), Entitlements =
                    []
            }, new Guild(new JsonGuild(), 1234, new RestClient(new RestClientConfiguration())),
            (_, _, _, _) => Task.CompletedTask, new RestClient(new RestClientConfiguration { })),
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

        
        var sut = await SlashCommandApi.XkcdCommandAsync(xkcdService, LoggerFactory, CommandContext, comicNumber);
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

        
        var sut = await SlashCommandApi.XkcdCommandAsync(xkcdService, LoggerFactory, CommandContext);
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
            .Respond(HttpStatusCode.OK, JsonContent.Create(new XkcdService.XkcdContent{Title = "Comic #1", Num = comicNumber, Alt = "Comic #1", Img = "some_url", Year = 2024, Month = 1, Day = 1}));
        
        var sut = await SlashCommandApi.XkcdCommandAsync(xkcdService, LoggerFactory, CommandContext, comicNumber);
        
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
            .Respond(HttpStatusCode.OK, JsonContent.Create(new XkcdService.XkcdContent{Title = $"Comic #{comicNumber}", Num = comicNumber, Alt = "Comic #1", Img = "some_url", Year = 2024, Month = 1, Day = 1}));
        
        var sut = await SlashCommandApi.XkcdCommandAsync(xkcdService, LoggerFactory, CommandContext);
        
        await Assert.That(sut.Embeds?.Count()).IsEqualTo(1);
        await Assert.That(sut.Embeds?.First().Title).Contains($"Latest comic #{comicNumber}");
    }
}