using Dotbot.Api.Queries;
using Dotbot.Api.Settings;
using Dotbot.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;

namespace Dotbot.Api.Services;

public interface ICustomCommandService
{
    Task<ServiceResult<GetCustomCommandResponse>> GetCustomCommandAsync(string guildId,
        string commandName);

    Task<ServiceResult<string>> SaveCustomCommandAsync(string guildId, string userId, string commandName,
        string? content = null, Attachment? attachment = null);
}

public class CustomCommandService(
    ILogger<CustomCommandService> logger,
    IGuildQueries guildQueries,
    IFileUploadService fileUploadService,
    IOptions<DiscordSettings> discordSettings,
    IGuildRepository guildRepository,
    HttpClient httpClient)
    : ICustomCommandService
{
    public async Task<ServiceResult<GetCustomCommandResponse>> GetCustomCommandAsync(string guildId, string commandName)
    {
        var customCommandsInServer = await guildQueries.GetAllCustomCommands(guildId);

        var matchingCommand = customCommandsInServer.FirstOrDefault(cc => cc.Name == commandName);
        if (matchingCommand is null)
            return ServiceResult<GetCustomCommandResponse>.Error($"No custom command exists matching '{commandName}'");

        var discordFileAttachments = new List<AttachmentProperties>();
        if (matchingCommand.Attachments.Count == 0)
            return ServiceResult<GetCustomCommandResponse>.Success(new GetCustomCommandResponse(matchingCommand.Name,
                matchingCommand.Content ?? "No content for this command", discordFileAttachments));

        var file = await fileUploadService.GetFile($"{discordSettings.Value.BucketEnvPrefix}-discord-{guildId}",
            matchingCommand.Attachments.First().Name, CancellationToken.None);
        if (file == null)
            return ServiceResult<GetCustomCommandResponse>.Error("Failed to retrieve the file for this command");

        using var memoryStream = new MemoryStream();
        await file.FileContent.CopyToAsync(memoryStream, CancellationToken.None);

        discordFileAttachments.Add(new AttachmentProperties(file.Filename, new MemoryStream(memoryStream.ToArray())));

        return ServiceResult<GetCustomCommandResponse>.Success(new GetCustomCommandResponse(matchingCommand.Name,
            matchingCommand.Content,
            discordFileAttachments));
    }

    public async Task<ServiceResult<string>> SaveCustomCommandAsync(string guildId, string userId, string commandName,
        string? content = null, Attachment? attachment = null)
    {
        logger.LogInformation("Saving custom command {command} with files: {files}", commandName, attachment != null);
        var guild = await guildRepository.GetByExternalIdAsync(guildId);

        if (guild is null)
            return ServiceResult<string>.Error($"No guild exists with id {guildId}");

        try
        {
            var customCommand = guild.CustomCommands.FirstOrDefault(cc => cc.Name == commandName);
            if (customCommand is not null)
            {
                customCommand.SetNewCommandContent(content, userId);
                foreach (var existingAttachment in customCommand.Attachments)
                    await fileUploadService.DeleteFile(
                        $"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", existingAttachment.Name,
                        CancellationToken.None);

                customCommand.DeleteAllAttachments();
                guildRepository.Update(guild);
            }
            else
            {
                customCommand = guild.AddCustomCommand(commandName, userId, content);
            }

            if (attachment != null)
            {
                var stream = await httpClient.GetStreamAsync(attachment.Url, CancellationToken.None);
                var fileExtension = Path.GetExtension(attachment.Url.Split("?")[0]);
                var attachmentName = $"{Guid.NewGuid()}{fileExtension}";
                await fileUploadService.UploadFile(
                    $"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", attachmentName, stream,
                    CancellationToken.None);

                customCommand.AddAttachment(attachmentName, fileExtension, attachment.Url);
            }

            await guildRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None);
            return ServiceResult<string>.SuccessWithMessage("Saved command successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError("{Exception}", ex.Message);
            return ServiceResult<string>.Error(ex.GetType().Name);
        }
    }
}

public class GetCustomCommandResponse
{
    public GetCustomCommandResponse(string name, string? content, List<AttachmentProperties>? attachments)
    {
        Name = name;
        Content = content;
        Attachments = attachments ?? [];
    }

    public string Name { get; set; }
    public string? Content { get; }
    public List<AttachmentProperties> Attachments { get; }
}