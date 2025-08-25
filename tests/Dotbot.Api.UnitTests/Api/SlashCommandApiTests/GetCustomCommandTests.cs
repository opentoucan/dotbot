using Dotbot.Api.Application.Api;
using Dotbot.Api.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NSubstitute;
using ServiceDefaults;

namespace Dotbot.Api.UnitTests.Api.SlashCommandApiTests;

public class GetCustomCommandTests
{
    private const ulong GuildId = 1234;
    private static readonly IRestRequestHandler RestRequestHandlerMock = Substitute.For<IRestRequestHandler>();
    private static readonly ILoggerFactory LoggerFactoryMock = Substitute.For<ILoggerFactory>();
    private static readonly IUserContext UserContextMock = Substitute.For<IUserContext>();
    private static readonly HttpApplicationCommandContext CommandContext = new(new SlashCommandInteraction(new JsonInteraction
    {
        GuildId = null,
        Data = new JsonInteractionData(),
        User = new JsonUser(),
        GuildUser = new JsonGuildUser(),
        Channel = new JsonChannel(),
        Entitlements =
                    []
    }, new Guild(new JsonGuild(), GuildId, new RestClient(new RestClientConfiguration()), IDictionaryProvider.OfDictionary),
            (_, _, _, _, _) => Task.FromResult<InteractionCallbackResponse?>(null), new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock })),
        new RestClient(new RestClientConfiguration()));

    private static readonly ICustomCommandService CustomCommandService = Substitute.For<ICustomCommandService>();
    private static readonly Instrumentation Instrumentation = new();

    [Test]
    public async Task CustomCommand_NoGuildId_DoesNotReturnsError()
    {
        var commandName = "test";
        await SlashCommandApi.GetCustomCommandAsync(CustomCommandService, Instrumentation, LoggerFactoryMock, CommandContext, commandName);

        await CustomCommandService.DidNotReceive().GetCustomCommandAsync(Arg.Any<string>(), commandName);
    }
}