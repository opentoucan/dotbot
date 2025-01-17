using Dotbot.Api.Application.Api;
using NetCord;
using NetCord.JsonModels;
using NetCord.Rest;

namespace Dotbot.Api.UnitTests.Api.SlashCommandApiTests;

public class AvatarCommandTests
{
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
            123, new RestClient());
        var globalAvatarFlag = true;
        var sut = SlashCommandApi.AvatarCommandAsync(guildUser, globalAvatarFlag);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.UserAvatar(userId, avatarHash, null).ToString());
    }
    
    [Test]
    public async Task AvatarCommand_GlobalFlagIsFalse_ReturnsGuildAvatarUrl()
    {
        ulong guildId = 123;
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildAvatarHash = "some_guild_hash";
        var guildUser = new GuildUser(new JsonGuildUser
            {
                User = new JsonUser { Id = userId, AvatarHash = avatarHash },
                GuildAvatarHash = guildAvatarHash
            },guildId, new RestClient());
        var globalAvatarFlag = false;
        var sut = SlashCommandApi.AvatarCommandAsync(guildUser, globalAvatarFlag);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.GuildUserAvatar(guildId, userId, guildAvatarHash, null).ToString());
    }
    
    [Test]
    public async Task AvatarCommand_GlobalFlagNotProvided_ReturnsGuildAvatarUrlByDefault()
    {
        ulong guildId = 123;
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildAvatarHash = "some_guild_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash },
            GuildAvatarHash = guildAvatarHash
        },guildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(guildUser);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.GuildUserAvatar(guildId, userId, guildAvatarHash, null).ToString());
    }
    
    [Test]
    public async Task AvatarCommand_GlobalFlagNotProvided_ReturnsGlobalAvatarUrlWhenNoGuildAvatar()
    {
        ulong guildId = 123;
        ulong userId = 999;
        var avatarHash = "some_global_hash";
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId, AvatarHash = avatarHash }
        },guildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(guildUser);

        await Assert.That(sut.Embeds?.FirstOrDefault()?.Image?.Url)
            .Contains(ImageUrl.UserAvatar(userId, avatarHash, null).ToString());
    }
    
    [Test]
    public async Task AvatarCommand_NoAvatarSet_ReturnsErrorMessage()
    {
        ulong guildId = 123;
        ulong userId = 999;
        var guildUser = new GuildUser(new JsonGuildUser
        {
            User = new JsonUser { Id = userId }
        },guildId, new RestClient());
        var sut = SlashCommandApi.AvatarCommandAsync(guildUser);

        await Assert.That(sut.Embeds).IsNull();
        await Assert.That(sut.Content).IsEqualTo("No avatar set");
    }
}