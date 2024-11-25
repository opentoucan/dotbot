using System.Text.Json;
using Discord;
using Dotbot.Gateway.Application.InteractionCommands.Exceptions;
using Dotbot.Gateway.Application.Queries;
using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Application.InteractionCommands.SlashCommands;

public class RetrieveCustomCommandHandler(
    IGuildQueries guildQueries,
    IFileUploadService fileUploadService,
    IOptions<Settings.Discord> discordSettings)
    : IRequestHandler<RetrieveCustomCommand, InteractionData>
{
    public async Task<InteractionData> Handle(RetrieveCustomCommand request, CancellationToken cancellationToken)
    {
        var guildId = request.GuildId;
        var customCommandsInServer = await guildQueries.GetAllCustomCommands(guildId);

        var matchingCommand = customCommandsInServer.FirstOrDefault(cc => cc.Name == request.CustomCommandName);
        if (matchingCommand is null)
            return new InteractionData($"No custom command exists matching '{request.CustomCommandName}'");
        var discordFileAttachments = new List<DiscordFileAttachment>();
        if (!(matchingCommand.Attachments?.Count > 0))
            return new InteractionData(matchingCommand?.Content, new List<Embed>(), discordFileAttachments);
        
        foreach (var attachment in matchingCommand.Attachments)
        {
            var file = await fileUploadService.GetFile($"{discordSettings.Value.BucketEnvPrefix}-discord-{guildId}", attachment.Name, cancellationToken);
            if (file == null) return new InteractionData("Failed to retrieve the file for this command");
            using var memoryStream = new MemoryStream();
            await file.FileContent.CopyToAsync(memoryStream, cancellationToken);
            discordFileAttachments.Add(new DiscordFileAttachment(file.Filename, "Custom command", false, memoryStream.ToArray()));
        }

        return new InteractionData(matchingCommand.Content, new List<Embed>(), discordFileAttachments);
    }
}

public class RetrieveCustomCommand : InteractionCommand
{
    public override string InteractionCommandName => "custom";
    
    public required string GuildId { get; set; }
    public string? DirectMessageChannelId { get; set; }
    public string CustomCommandName { get; set; } = null!;
    
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        CustomCommandName = ((JsonElement?)interactionRequest.Data?.Options?.FirstOrDefault()?.Value)?.GetString() 
                            ?? throw new CommandValidationException("Custom command name must be passed");
        GuildId = interactionRequest.Guild?.Id ?? throw new CommandValidationException("Command must be used within a guild");
        DirectMessageChannelId = interactionRequest.User?.Id;
        if (string.IsNullOrWhiteSpace(GuildId) && string.IsNullOrWhiteSpace(DirectMessageChannelId))
            throw new CommandValidationException("Custom command must be used inside a server or in a direct message");
    }
}