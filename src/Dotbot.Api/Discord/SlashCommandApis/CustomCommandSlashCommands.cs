using System.Diagnostics;
using Dotbot.Api.Discord.Modules;
using Dotbot.Api.Services;
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
        IHttpInteractionCommandLogger httpInteractionCommandLogger,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "command", Description = "Name of the custom command",
            AutocompleteProviderType = typeof(CustomCommandAutocompleteProvider))]
        string commandName)
    {
        using var activity = Instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

        var cancellationTokenSource = new CancellationTokenSource();

        var guildId = context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message("Cannot use this command outside of a server"),
                cancellationToken: cancellationTokenSource.Token);
            return;
        }

        await httpInteractionCommandLogger.LogCommand("custom", commandName, guildId,
            context.User.Id.ToString(), cancellationTokenSource.Token);

        await context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);

        var logger = loggerFactory.CreateLogger("SlashCommandApi");
        logger.LogInformation("Member {member} is retrieving custom command {command}", context.User.Username,
            commandName);

        var customCommandResponse = await customCommandService.GetCustomCommandAsync(guildId, commandName);

        if (!customCommandResponse.IsSuccess)
            await context.Interaction.SendFollowupMessageAsync(customCommandResponse.ErrorResult!.ErrorMessage,
                cancellationToken: cancellationTokenSource.Token);
        else
            await context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
                    .WithContent(customCommandResponse.Value!.Content)
                    .AddAttachments(customCommandResponse.Value.Attachments),
                cancellationToken: cancellationTokenSource.Token);
    }
}