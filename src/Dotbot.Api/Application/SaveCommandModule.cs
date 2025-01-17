using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Api.Application;

[SlashCommand("save", "Save a new custom command")]
public class SaveCommandModule(ICustomCommandService customCommandService) : ApplicationCommandModule<HttpApplicationCommandContext>
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
        var guildId = Context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.Message("Cannot use this command outside of a server") );
            return;
        }
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
        
        var result = await customCommandService.SaveCustomCommandAsync(guildId, Context.Interaction.User.Id.ToString(), commandName, content, file);
        if (!result.IsSuccess)
            await Context.Interaction.SendFollowupMessageAsync(string.Join(" ", result.Errors));
        else
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties().WithContent(result.SuccessMessage));
    }
}