using System.Diagnostics;
using Dotbot.Api.Application;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.SlashCommandApis;

public static class CustomCommandSlashCommands
{
    public static async Task FetchCustomCommandAsync(
        ICustomCommandService customCommandService,
        Instrumentation instrumentation,
        ILoggerFactory loggerFactory,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "command", Description = "Name of the custom command",
            AutocompleteProviderType = typeof(CustomCommandAutocompleteProvider))]
        string commandName)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

        var guildId = context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message("Cannot use this command outside of a server"));
            return;
        }

        await context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        var logger = loggerFactory.CreateLogger("SlashCommandApi");
        logger.LogInformation("Member {member} is retrieving custom command {command}", context.User.Username,
            commandName);

        var customCommandResponse = await customCommandService.GetCustomCommandAsync(guildId, commandName);

        if (!customCommandResponse.IsSuccess)
        {
            await context.Interaction.SendFollowupMessageAsync(customCommandResponse.ErrorResult!.ErrorMessage);
        }
        else
        {
            await context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
                .WithContent(customCommandResponse.Value!.Content)
                .AddAttachments(customCommandResponse.Value.Attachments));

            var displayName = (context.User as GuildUser)?.Nickname ?? context.User.GlobalName ?? context.User.Username;
            var tags = new TagList
            {
                { "guild_id", guildId },
                { "interaction_name", "custom" },
                { "custom_command_name", commandName },
                { "member_id", context.User.Id },
                { "member_display_name", displayName }
            };

            foreach (var tag in tags)
                activity?.SetTag(tag.Key, tag.Value);

            instrumentation.CustomCommandsFetchedCounter.Add(1, tags);
        }
    }
}