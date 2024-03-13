using System.Text.Json;
using Bot.Gateway.Application.InteractionCommands.Exceptions;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using MediatR;
using Path = System.IO.Path;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class SaveCustomCommandHandler : IRequestHandler<SaveCustomCommand, InteractionData>
{
    private readonly ILogger<SaveCustomCommandHandler> _logger;
    private readonly DbContext _dbContext;
    private readonly IBotCommandRepository _botCommandRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly IHttpClientFactory _httpClientFactory;

    public SaveCustomCommandHandler(ILogger<SaveCustomCommandHandler> logger, DbContext dbContext, IBotCommandRepository botCommandRepository, IFileUploadService fileUploadService, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _botCommandRepository = botCommandRepository;
        _fileUploadService = fileUploadService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<InteractionData> Handle(SaveCustomCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating HTTP Client in {handler}", nameof(SaveCustomCommandHandler));
        var client = _httpClientFactory.CreateClient();
        
        var contextId = request.GuildId ?? request.DirectMessageChannelId!;
    
        _logger.LogInformation("Saving custom command {command} with {files} files", request.CustomCommandName, request.FileNameUrlDictionary.Count);
        var botCommand = new BotCommand(contextId,
            request.CustomCommandName,
                request.SenderId,
            request.TextContent,
            request.FileNameUrlDictionary.Keys.ToList());

        try
        {
            _logger.LogDebug("Opening transaction for bot command {botCommand}", botCommand.Name);
            await _dbContext.BeginTransactionAsync(cancellationToken);

            foreach (var item in request.FileNameUrlDictionary)
            {
                var stream = await client.GetStreamAsync(new Uri(item.Value), cancellationToken);
                await _fileUploadService.UploadFile($"discord-{contextId}", item.Key, stream);
            }
            _logger.LogDebug("Saving bot command {botCommand}", botCommand.Name);
            await _botCommandRepository.SaveCommand(botCommand);
            _logger.LogDebug("Committing transaction for bot command {botCommand}", botCommand.Name);
            await _dbContext.CommitTransactionAsync(cancellationToken);
            return new ("Saved command");
        }
        catch (Exception ex)
        {
            _logger.LogError("{Exception}", ex.Message);
            await _dbContext.RollbackTransactionAsync(cancellationToken);
            return new ("Failed to save command");
        }
    }
}

public class SaveCustomCommand : InteractionCommand
{
    public override string InteractionCommandName => "save";
    
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        var commandOptions = interactionRequest.Data?.Options?.FirstOrDefault()?.SubOptions;
        CustomCommandName = ((JsonElement?)commandOptions?.FirstOrDefault(o => o.Name == "name")?.Value)?.GetString() 
                            ?? throw new CommandValidationException("Custom command must have a name");
        
        TextContent = ((JsonElement?)commandOptions?.FirstOrDefault(o => o.Name == "text")?.Value)?.GetString();
        GuildId = interactionRequest.Guild?.Id;
        DirectMessageChannelId = interactionRequest.User?.Id;
        if (string.IsNullOrWhiteSpace(GuildId) && string.IsNullOrWhiteSpace(DirectMessageChannelId))
            throw new CommandValidationException("Custom command must be used inside a server or in a direct message");
        
        if (interactionRequest.Data?.Resolved?.Attachments?.Any(x => (x.Value?.Size / 1024 / 1024) > 25) ?? true)
            throw new CommandValidationException("File is too large, it must be less than 25MB");

        SenderId = interactionRequest.Member?.User.Id ?? interactionRequest.User!.Id!;
        FileNameUrlDictionary = interactionRequest.Data?.Resolved?.Attachments?
            .Select((value, i) => (value, i))
            .ToDictionary(
                x => $"{CustomCommandName}_{x.i}{Path.GetExtension(x.value.Value.Url).Split("?")[0]}", 
                x => x.value.Value.Url) ?? new Dictionary<string, string>();
    }

    public string CustomCommandName { get; private set; } = null!;
    public string? TextContent { get; private set; }
    public string? GuildId { get; private set; }
    public string? DirectMessageChannelId { get; private set; }
    public string SenderId { get; private set; } = null!;
    public IDictionary<string, string> FileNameUrlDictionary { get; private set; } = null!;
}