using System.Diagnostics;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.SlashCommandApis;

public static class AvatarSlashCommands
{
    public static InteractionMessageProperties FetchAvatarAsync(
        Instrumentation instrumentation,
        HttpApplicationCommandContext context,
        GuildUser user,
        [SlashCommandParameter(Name = "global", Description = "Optional flag if you want the user's global avatar instead of the server")] bool globalAvatar = false)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("AvatarCommand", ActivityKind.Client);
        var userAvatarUrl = user.GetAvatarUrl();
        var guildAvatarUrl = user.GetGuildAvatarUrl();

        var avatarUrl = globalAvatar ? userAvatarUrl : guildAvatarUrl ?? userAvatarUrl;

        var displayName = user.Nickname ?? user?.GlobalName ?? user?.Username;
        var tags = new TagList
        {
            { "guild_id", user?.GuildId },
            { "interaction_name", "avatar" },
            { "member_id", user?.Id },
            { "member_display_name", displayName }
        };

        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);
        instrumentation.AvatarCounter.Add(1, tags);

        var embed = new EmbedProperties()
            .WithTitle("Avatar")
            .WithImage(new EmbedImageProperties(avatarUrl?.ToString(512)))
            .WithDescription(displayName);

        return avatarUrl is null ? "No avatar set" : new InteractionMessageProperties().AddEmbeds(embed);
    }
}