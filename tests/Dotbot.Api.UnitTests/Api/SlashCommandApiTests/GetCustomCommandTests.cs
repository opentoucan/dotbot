using Dotbot.Api.Application.Api;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NSubstitute;

namespace Dotbot.Api.UnitTests.Api.SlashCommandApiTests;

public class GetCustomCommandTests
{
    private const ulong GuildId = 1234;
    private static readonly IRestRequestHandler RestRequestHandlerMock = Substitute.For<IRestRequestHandler>();
    private static readonly HttpApplicationCommandContext CommandContext = new(new SlashCommandInteraction(new JsonInteraction
    {
        GuildId = null,
        Data = new JsonInteractionData(),
        User = new JsonUser(),
        GuildUser = new JsonGuildUser(),
        Channel = new JsonChannel(),
        Entitlements =
                    []
    }, new Guild(new JsonGuild(), GuildId, new RestClient(new RestClientConfiguration())),
            (_, _, _, _) => Task.CompletedTask, new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock })),
        new RestClient(new RestClientConfiguration()));

    private static readonly ICustomCommandService CustomCommandService = Substitute.For<ICustomCommandService>();

    [Test]
    public async Task CustomCommand_NoGuildId_DoesNotReturnsError()
    {
        var commandName = "test";
        await SlashCommandApi.GetCustomCommandAsync(CustomCommandService, CommandContext, commandName);

        await CustomCommandService.DidNotReceive().GetCustomCommandAsync(Arg.Any<string>(), commandName);
    }
}