using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Services;
using Dotbot.Gateway.Settings;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Application.InteractionCommands.SlashCommands;

public class SaveCustomCommandHandler : IRequestHandler<SaveCustomCommand, InteractionData>
{
    private readonly ILogger<SaveCustomCommandHandler> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DiscordSettings _discordSettings;
    private readonly ICustomCommandRepository _customCommandRepository;

    public SaveCustomCommandHandler(
        ILogger<SaveCustomCommandHandler> logger,
        IFileUploadService fileUploadService, 
        IHttpClientFactory httpClientFactory, 
        IOptions<DiscordSettings> discordSettings,
        ICustomCommandRepository customCommandRepository)
    {
        _logger = logger;
        _fileUploadService = fileUploadService;
        _httpClientFactory = httpClientFactory;
        _customCommandRepository = customCommandRepository;
        _discordSettings = discordSettings.Value;
    }

    public async Task<InteractionData> Handle(SaveCustomCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating HTTP Client in {handler}", nameof(SaveCustomCommandHandler));
        var client = _httpClientFactory.CreateClient();
        
        var contextId = request.GuildId ?? request.DirectMessageChannelId!;
    
        _logger.LogInformation("Saving custom command {command} with {files} files", request.CustomCommandName, request.FileNameUrlDictionary.Count);

        var customCommand = await _customCommandRepository.GetByNameAsync(request.CustomCommandName);
        if (customCommand is not null)
        {
            customCommand.SetNewCommandContent(request.TextContent, request.SenderId);
            _customCommandRepository.Update(customCommand);
        }
        else
        {
            customCommand = new CustomCommand(request.CustomCommandName, request.SenderId, new Guild(contextId, !string.IsNullOrWhiteSpace(request.DirectMessageChannelId)), request.TextContent);
            _customCommandRepository.Add(customCommand);
        }

        try
        {
            foreach (var item in request.FileNameUrlDictionary)
            {
                var stream = await client.GetStreamAsync(new Uri(item.Value), cancellationToken);
                await _fileUploadService.UploadFile($"{_discordSettings.BucketEnvPrefix}-discord-{contextId}", item.Key, stream);
                customCommand.AddAttachment(item.Key, Path.GetExtension(item.Key), item.Value);
            }
            await _customCommandRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return new InteractionData("Saved command");
        }
        catch (Exception ex)
        {
            _logger.LogError("{Exception}", ex.Message);
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
    public string? DirectMessageChannelId { get; set; }
    public string SenderId { get; set; } = null!;
    public IDictionary<string, string> FileNameUrlDictionary { get; set; } = new Dictionary<string, string>();
}