using Dotbot.Api.Application.Api;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NSubstitute;
using ServiceDefaults;

namespace Dotbot.Api.UnitTests.Api.SlashCommandApiTests;

public class AvatarCommandTests
{
    private static ulong GuildId => 123;
    private static readonly Instrumentation Instrumentation = new();
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

    [Test]
    public async Task AvatarCommand_GlobalFlagIsTrue_ReturnsGlobalUrl()
    {
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildAvatarHash = "some_guild_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash },
            GuildAvatarHash = guildAvatarHash
        },
            GuildId, new RestClient());
        var globalAvatarFlag = true;
        var sut = SlashCommandApi.AvatarCommandAsync(Instrumentation, CommandContext, guildUser, globalAvatarFlag);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.UserAvatar(userId, avatarHash, null).ToString());
    }

    [Test]
    public async Task AvatarCommand_GlobalFlagIsFalse_ReturnsGuildAvatarUrl()
    {
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildAvatarHash = "some_guild_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash },
            GuildAvatarHash = guildAvatarHash
        }, GuildId, new RestClient());
        var globalAvatarFlag = false;
        var sut = SlashCommandApi.AvatarCommandAsync(Instrumentation, CommandContext, guildUser, globalAvatarFlag);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.GuildUserAvatar(GuildId, userId, guildAvatarHash, null).ToString());
    }

    [Test]
    public async Task AvatarCommand_GlobalFlagNotProvided_ReturnsGuildAvatarUrlByDefault()
    {
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildAvatarHash = "some_guild_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash },
            GuildAvatarHash = guildAvatarHash
        }, GuildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(Instrumentation, CommandContext, guildUser);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.GuildUserAvatar(GuildId, userId, guildAvatarHash, null).ToString());
    }

    [Test]
    public async Task AvatarCommand_GlobalFlagNotProvided_ReturnsGlobalAvatarUrlWhenNoGuildAvatar()
    {
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash }
        }, GuildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(Instrumentation, CommandContext, guildUser);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.UserAvatar(userId, avatarHash, null).ToString());
    }

    [Test]
    public async Task AvatarCommand_NoAvatarSet_ReturnsErrorMessage()
    {
        ulong userId = 999;
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId }
        }, GuildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(Instrumentation, CommandContext, guildUser);

        await Assert.That(sut.Embeds).IsNull();
        await Assert.That(sut.Content).IsEqualTo("No avatar set");
    }
}