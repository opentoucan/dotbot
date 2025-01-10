using Dotbot.Gateway.Services;
using Dotbot.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Gateway.Application;

[SlashCommand("save", "Save a new custom command")]
public class SaveCommandModule(
    IFileUploadService fileUploadService,
    IOptions<Settings.Discord> discordSettings,
    IHttpClientFactory httpClientFactory,
    IGuildRepository guildRepository,
    ILogger<DiscordCommandsModule> logger) : ApplicationCommandModule<HttpApplicationCommandContext>
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
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());


        logger.LogDebug("Creating HTTP Client in {handler}", nameof(DiscordCommandsModule));
        var client = httpClientFactory.CreateClient();

        logger.LogInformation("Saving custom command {command} with files {files}", commandName, file == null);

        var guild = await guildRepository.GetByExternalIdAsync(Context.Interaction.GuildId!.GetValueOrDefault()
            .ToString());
        if (guild is null)
            throw new Exception("Guild not found in registered guilds");

        try
        {
            var customCommand = guild.CustomCommands.FirstOrDefault(cc => cc.Name == commandName);
            if (customCommand is not null)
            {
                customCommand.SetNewCommandContent(content, Context.User.Id.ToString());
                foreach (var attachment in customCommand.Attachments)
                {
                    await fileUploadService.DeleteFile(
                        $"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", attachment.Name,
                        CancellationToken.None);
                }

                customCommand.DeleteAllAttachments();
                guildRepository.Update(guild);
            }
            else
            {
                customCommand = guild.AddCustomCommand(commandName, Context.User.Id.ToString(), content);
            }

            if (file != null)
            {
                var stream = await client.GetStreamAsync(file.Url, CancellationToken.None);
                var attachmentName = $"{Guid.NewGuid()}{Path.GetExtension(file.Url.Split("?")[0])}";
                await fileUploadService.UploadFile(
                    $"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", attachmentName, stream,
                    CancellationToken.None);
                customCommand.AddAttachment(attachmentName, Path.GetExtension(file.FileName), file.Url);
            }

            await guildRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None);
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            { Content = "Saved command successfully!" });
        }
        catch (Exception ex)
        {
            logger.LogError("{Exception}", ex.Message);
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            { Content = "Failed to save command!" });
        }
    }
}