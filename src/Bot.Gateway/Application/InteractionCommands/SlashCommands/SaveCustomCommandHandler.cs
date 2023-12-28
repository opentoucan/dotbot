using System.Text.Json;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Model.Responses.Discord;
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
        var client = _httpClientFactory.CreateClient();
        var filesList = request.Data.Data?.Resolved?.Attachments?.Select((value, i) => (value, i)).ToList() ?? [];

        var commandOptions = request.Data.Data!.Options!.FirstOrDefault()!.SubOptions!;
        var nameJson = (JsonElement)commandOptions.FirstOrDefault(o => o.Name == "name")!.Value!;
        var textJson = (JsonElement?)commandOptions.FirstOrDefault(o => o.Name == "text")?.Value;
        var customCommandName = nameJson.GetString();
        var customCommandText = textJson?.GetString() ?? null;
        var botCommand = new BotCommand(request.Data.GuildId!,
            customCommandName!,
                request.Data.Member!.User.Id!,
            customCommandText,
            filesList.Select(item => $"{customCommandName}_{item.i}{Path.GetExtension(item.value.Value.Url).Split("?")[0]}").ToList());

        try
        {
            await _dbContext.BeginTransactionAsync();
            await _botCommandRepository.SaveCommand(botCommand);

            foreach (var item in filesList)
            {
                var stream = await client.GetStreamAsync(new Uri(item.value.Value.Url), cancellationToken);
                var extension = Path.GetExtension(item.value.Value.Url);
                await _fileUploadService.UploadFile(request.Data.GuildId!, $"{botCommand.Name}_{item.i}{extension.Split("?")[0]}", stream);
            }

            await _dbContext.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("{0}", ex.Message);
            throw;
        }
        finally
        {
            _dbContext.RollbackTransaction();
        }

        return new InteractionData("Command Custom");
    }
}