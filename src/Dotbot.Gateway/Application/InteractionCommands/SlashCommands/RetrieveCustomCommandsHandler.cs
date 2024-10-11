using Discord;
using Dotbot.Gateway.Application.Queries;
using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Application.InteractionCommands.SlashCommands;

public class RetrieveCustomCommandHandler : IRequestHandler<RetrieveCustomCommand, InteractionData>
{
    private readonly ICustomCommandQueries _customCommandQueries;
    private readonly IFileUploadService _fileUploadService;
    private readonly Settings.Discord _discord;
    public RetrieveCustomCommandHandler(
        ICustomCommandQueries customCommandQueries, 
        IFileUploadService fileUploadService,
        IOptions<Settings.Discord> discordSettings)
    {
        _customCommandQueries = customCommandQueries;
        _fileUploadService = fileUploadService;
        _discord = discordSettings.Value;
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
            var file = await _fileUploadService.GetFile($"{_discord.BucketEnvPrefix}-discord-{contextId}", attachment.Name);
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