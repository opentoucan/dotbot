using System.Text.Json;
using Bot.Gateway.Application.InteractionCommands.Exceptions;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class RetrieveCustomCommandHandler : IRequestHandler<RetrieveCustomCommand, InteractionData>
{
    private readonly IBotCommandRepository _botCommandRepository;
    private readonly IFileUploadService _fileUploadService;

    public RetrieveCustomCommandHandler(IBotCommandRepository botCommandRepository, IFileUploadService fileUploadService)
    {
        _botCommandRepository = botCommandRepository;
        _fileUploadService = fileUploadService;
    }

    public async Task<InteractionData> Handle(RetrieveCustomCommand request, CancellationToken cancellationToken)
    {
        var contextId = request.GuildId ?? request.DirectMessageChannelId!;
        var botCommand = _botCommandRepository.GetCommand(contextId,request.CustomCommandName);
        if (botCommand is null)
            return new InteractionData($"No custom command exists matching '{request.CustomCommandName}'");
        var discordFileAttachments = new List<DiscordFileAttachment>();
        if (!(botCommand?.AttachmentIds?.Count > 0))
            return new InteractionData(botCommand?.Content, new List<Embed>(), discordFileAttachments);
        
        foreach (var attachmentId in botCommand.AttachmentIds)
        {
            var file = await _fileUploadService.GetFile($"discord-{contextId}", attachmentId);
            if (file == null) return new InteractionData("Failed to retrieve the file for this command");
            using var memoryStream = new MemoryStream();
            await file.FileContent.CopyToAsync(memoryStream, cancellationToken);
            discordFileAttachments.Add(new DiscordFileAttachment(file.Filename, "Custom command", false, memoryStream.ToArray()));
        }

        return new InteractionData(botCommand.Content, new List<Embed>(), discordFileAttachments);
    }
}

public class RetrieveCustomCommand : InteractionCommand
{
    public override string InteractionCommandName => "custom";
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        CustomCommandName = ((JsonElement?)interactionRequest.Data?.Options?.FirstOrDefault()?.Value)?.GetString() 
                            ?? throw new CommandValidationException("Custom command name must be passed");
        GuildId = interactionRequest.Data?.GuildId;
        DirectMessageChannelId = interactionRequest.User?.Id;
        if (string.IsNullOrWhiteSpace(GuildId) && string.IsNullOrWhiteSpace(DirectMessageChannelId))
            throw new CommandValidationException("Custom command must be used inside a server or in a direct message");
    }
    
    public string? GuildId { get; private set; }
    public string? DirectMessageChannelId { get; private set; }
    public string CustomCommandName { get; private set; } = null!;
}