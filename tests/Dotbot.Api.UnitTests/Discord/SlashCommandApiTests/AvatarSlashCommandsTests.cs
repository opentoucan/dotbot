using Dotbot.Api.Discord.SlashCommandApis;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NSubstitute;
using ServiceDefaults;

namespace Dotbot.Api.UnitTests.Discord.SlashCommandApiTests;

public class AvatarSlashCommandsTests
{
    private static ulong GuildId => 123;
    private static readonly Instrumentation Instrumentation = new();
    private static readonly IRestRequestHandler RestRequestHandlerMock = Substitute.For<IRestRequestHandler>();
#pragma warning disable TUnit0023 // Member should be disposed within a clean up method
    private static readonly HttpApplicationCommandContext CommandContext = new(new SlashCommandInteraction(new JsonInteraction
#pragma warning restore TUnit0023 // Member should be disposed within a clean up method
    {
        GuildId = null,
        Data = new JsonInteractionData(),
        User = new JsonUser(),
        GuildUser = new JsonGuildUser(),
        Channel = new JsonChannel(),
        Entitlements =
                    []
    }, new Guild(new JsonGuild(), GuildId, new RestClient(new RestClientConfiguration()), IDictionaryProvider.OfDictionary),
            (_, _, _, _, _) => Task.FromResult<InteractionCallbackResponse?>(new InteractionCallbackResponse(new NetCord.Rest.JsonModels.JsonInteractionCallbackResponse(), new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock }))), new RestClient(new RestClientConfiguration { RequestHandler = RestRequestHandlerMock })),
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
        var sut = AvatarSlashCommands.FetchAvatarAsync(Instrumentation, CommandContext, guildUser, globalAvatarFlag);

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
        var sut = AvatarSlashCommands.FetchAvatarAsync(Instrumentation, CommandContext, guildUser, globalAvatarFlag);

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
        var sut = AvatarSlashCommands.FetchAvatarAsync(Instrumentation, CommandContext, guildUser);

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
        var sut = AvatarSlashCommands.FetchAvatarAsync(Instrumentation, CommandContext, guildUser);

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
        var sut = AvatarSlashCommands.FetchAvatarAsync(Instrumentation, CommandContext, guildUser);

        await Assert.That(sut.Embeds).IsNull();
        await Assert.That(sut.Content).IsEqualTo("No avatar set");
    }
}