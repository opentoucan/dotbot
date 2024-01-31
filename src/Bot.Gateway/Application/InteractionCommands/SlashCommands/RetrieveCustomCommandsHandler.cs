using System.Text.Json;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Model.Requests.Discord;
using Bot.Gateway.Model.Responses.Discord;
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
        var customCommandJson = (JsonElement)request.Data.Data!.Options!.FirstOrDefault()!.Value!;
        var customCommandName = customCommandJson.GetString();
        var botCommand = _botCommandRepository.GetCommand(request.Data.GuildId!,customCommandName!);
        if (botCommand is null)
            return new InteractionData($"No custom command exists matching '{customCommandName}'");
        var discordFileAttachments = new List<DiscordFileAttachment>();
        if (botCommand?.AttachmentIds?.Count > 0)
        {
            foreach (var attachmentId in botCommand.AttachmentIds)
            {
                var file = await _fileUploadService.GetFile($"discord-{request.Data.GuildId!}", attachmentId);
                using var memoryStream = new MemoryStream();
                await file.FileContent.CopyToAsync(memoryStream, cancellationToken);
                discordFileAttachments.Add(new DiscordFileAttachment(file.Filename, "Custom command", false, memoryStream.ToArray()));   
            }
        }

        return new InteractionData(botCommand?.Content, new List<Embed>(), discordFileAttachments);
    }
}

public class RetrieveCustomCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Custom;
    public override InteractionRequest Data { get; set; } = null!;
}