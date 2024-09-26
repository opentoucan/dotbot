using System.Text.Json;
using Discord;
using Bot.Gateway.Application.InteractionCommands.Exceptions;
using Bot.Gateway.Application.Queries;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Services;
using Bot.Gateway.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class RetrieveCustomCommandHandler : IRequestHandler<RetrieveCustomCommand, InteractionData>
{
    private readonly ICustomCommandQueries _customCommandQueries;
    private readonly IFileUploadService _fileUploadService;
    private readonly DiscordSettings _discordSettings;
    public RetrieveCustomCommandHandler(
        ICustomCommandQueries customCommandQueries, 
        IFileUploadService fileUploadService,
        IOptions<DiscordSettings> discordSettings)
    {
        _customCommandQueries = customCommandQueries;
        _fileUploadService = fileUploadService;
        _discordSettings = discordSettings.Value;
    }

    public async Task<InteractionData> Handle(RetrieveCustomCommand request, CancellationToken cancellationToken)
    {
        var contextId = request.GuildId ?? request.DirectMessageChannelId!;
        var customCommandsInServer = await _customCommandQueries.GetCustomCommandsFromServerAsync(contextId);

        var matchingCommand = customCommandsInServer.FirstOrDefault(cc => cc.Name == request.CustomCommandName);
        if (matchingCommand is null)
            return new InteractionData($"No custom command exists matching '{request.CustomCommandName}'");
        var discordFileAttachments = new List<DiscordFileAttachment>();
        if (!(matchingCommand.Attachments?.Count > 0))
            return new InteractionData(matchingCommand?.Content, new List<Embed>(), discordFileAttachments);
        
        foreach (var attachment in matchingCommand.Attachments)
        {
            var file = await _fileUploadService.GetFile($"{_discordSettings.BucketEnvPrefix}-discord-{contextId}", attachment.Name);
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
    
    public string? GuildId { get; set; }
    public string? DirectMessageChannelId { get; set; }
    public string CustomCommandName { get; set; } = null!;
}