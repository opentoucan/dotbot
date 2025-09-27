using System.Diagnostics;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.Modules;

[SlashCommand("save", "Save a new custom command")]
public class SaveCommandModule(
    ICustomCommandService customCommandService,
    IHttpInteractionCommandLogger httpInteractionCommandLogger)
    : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SubSlashCommand("attachment", "Attachment to save with a custom command")]
    public async Task SaveAttachmentCommand(
        [SlashCommandParameter(Name = "command", Description = "Name of the command to save")]
        string command,
        [SlashCommandParameter(Name = "attachment", Description = "Attachment to save with the command")]
        Attachment attachment,
        [SlashCommandParameter(Name = "text", Description = "Text content of the command to save")]
        string? text = null)
    {
        await SaveCommandAsync(command, text, attachment);
    }

    [SubSlashCommand("text", "Text content to save with a custom command")]
    public async Task SaveTextCommand(
        [SlashCommandParameter(Name = "command", Description = "Name of the command to save")]
        string command,
        [SlashCommandParameter(Name = "text", Description = "Text content of the command to save")]
        string text,
        [SlashCommandParameter(Name = "attachment", Description = "Attachment to save with the command")]
        Attachment? attachment = null)
    {
        await SaveCommandAsync(command, text, attachment);
    }


    private async Task SaveCommandAsync(string commandName, string? content = null, Attachment? file = null)
    {
        using var activity = Instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

        var cancellationTokenSource = new CancellationTokenSource();

        var guildId = Context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message("Cannot use this command outside of a server"),
                cancellationToken: cancellationTokenSource.Token);
            return;
        }

        await httpInteractionCommandLogger.LogCommand(commandName, content ?? "file", guildId,
            Context.User.Id.ToString(), cancellationTokenSource.Token);

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);


        var result = await customCommandService.SaveCustomCommandAsync(guildId, Context.Interaction.User.Id.ToString(),
            commandName, content, file);
        if (!result.IsSuccess)
        {
            await Context.Interaction.SendFollowupMessageAsync(
                "An error occurred while saving the command. It has not been saved.",
                cancellationToken: cancellationTokenSource.Token);

            Instrumentation.ExceptionCounter.Add(1,
                new KeyValuePair<string, object?>("type", result.ErrorResult?.ErrorMessage),
                new KeyValuePair<string, object?>("interaction_name", "save"),
                new KeyValuePair<string, object?>("guild_id", guildId)
            );
        }
        else
        {
            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties().WithContent(result.Message),
                cancellationToken: cancellationTokenSource.Token);
            var displayName = (Context.Interaction.User as GuildUser)?.Nickname ??
                              Context.Interaction.User.GlobalName ?? Context.Interaction.User.Username;

            var tags = new TagList
            {
                { "guild_id", guildId },
                { "command_name", "save" },
                { "custom_command_name", commandName },
                { "member_id", Context.Interaction.User.Id },
                { "member_display_name", displayName }
            };

            foreach (var tag in tags)
                activity?.SetTag(tag.Key, tag.Value);

            Instrumentation.SavedCustomCommandsCounter.Add(1, tags);
        }
    }
}