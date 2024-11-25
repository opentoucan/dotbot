using System.Text.Json;
using Dotbot.Gateway.Application.InteractionCommands.Exceptions;
using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Services;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Options;
using Guild = Dotbot.Infrastructure.Entities.Guild;

namespace Dotbot.Gateway.Application.InteractionCommands.SlashCommands;

public class SaveCustomCommandHandler(
    ILogger<SaveCustomCommandHandler> logger,
    IFileUploadService fileUploadService,
    IHttpClientFactory httpClientFactory,
    IOptions<Settings.Discord> discordSettings,
    IGuildRepository guildRepository)
    : IRequestHandler<SaveCustomCommand, InteractionData>
{
    public async Task<InteractionData> Handle(SaveCustomCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Creating HTTP Client in {handler}", nameof(SaveCustomCommandHandler));
        var client = httpClientFactory.CreateClient();

        logger.LogInformation("Saving custom command {command} with {files} files", request.CustomCommandName, request.FileNameUrlDictionary.Count);

        var guild = await guildRepository.GetByExternalIdAsync(request.GuildId!);
        if (guild is null)
            throw new Exception("Guild not found in registered guilds");

        try
        {
            var customCommand = guild.CustomCommands.FirstOrDefault(cc => cc.Name == request.CustomCommandName);
            if (customCommand is not null)
            {
                customCommand.SetNewCommandContent(request.TextContent, request.SenderId);
                foreach (var attachment in customCommand.Attachments)
                {
                    await fileUploadService.DeleteFile($"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", attachment.Name, cancellationToken);
                }

                customCommand.DeleteAllAttachments();
                guildRepository.Update(guild);
            }
            else
            {
                customCommand = guild.AddCustomCommand(request.CustomCommandName, request.SenderId, request.TextContent);
            }

            foreach (var item in request.FileNameUrlDictionary)
            {
                var stream = await client.GetStreamAsync(new Uri(item.Value), cancellationToken);
                await fileUploadService.UploadFile($"{discordSettings.Value.BucketEnvPrefix}-discord-{guild.ExternalId}", item.Key, stream, cancellationToken);
                customCommand.AddAttachment(item.Key, Path.GetExtension(item.Key), item.Value);
            }

            await guildRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return new InteractionData("Saved command");
        }
        catch (Exception ex)
        {
            logger.LogError("{Exception}", ex.Message);
            return new InteractionData("Failed to save command");
        }
    }
}

public class SaveCustomCommand : InteractionCommand
{
    public override string InteractionCommandName => "save";
    public string CustomCommandName { get; set; } = null!;
    public string? TextContent { get; set; }
    public string? GuildId { get; set; }
    public string SenderId { get; set; } = null!;
    public IDictionary<string, string> FileNameUrlDictionary { get; set; } = new Dictionary<string, string>();

    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        var commandOptions = interactionRequest.Data?.Options?.FirstOrDefault()?.SubOptions;
        CustomCommandName = ((JsonElement?)commandOptions?.FirstOrDefault(o => o.Name == "name")?.Value)?.GetString()
                            ?? throw new CommandValidationException("Custom command must have a name");

        TextContent = ((JsonElement?)commandOptions?.FirstOrDefault(o => o.Name == "text")?.Value)?.GetString();
        GuildId = interactionRequest.Guild?.Id;
        if (string.IsNullOrWhiteSpace(GuildId))
            throw new CommandValidationException("Custom command must be used inside a server");

        if (interactionRequest.Data?.Resolved?.Attachments != null &&
            interactionRequest.Data.Resolved.Attachments.Any(x => x.Value.Size / 1000000 > 25))
            throw new CommandValidationException("File is too large, it must be less than 25MB");

        SenderId = interactionRequest.Member?.User.Id ?? interactionRequest.User!.Id!;
        FileNameUrlDictionary = interactionRequest.Data?.Resolved?.Attachments?
            .Select((value, i) => (value, i))
            .ToDictionary(
                x => $"{CustomCommandName}_{x.i}{Path.GetExtension(x.value.Value.Url).Split("?")[0]}",
                x => x.value.Value.Url) ?? new Dictionary<string, string>();
    }
}