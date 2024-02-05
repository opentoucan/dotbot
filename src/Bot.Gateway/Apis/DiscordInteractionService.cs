using Bot.Gateway.Application.InteractionCommands;
using Bot.Gateway.Infrastructure.HttpClient;
using Bot.Gateway.Infrastructure.Repositories;
using MediatR;

namespace Bot.Gateway.Apis;

public class DiscordInteractionService(
    IDiscordHttpRequestHelper discordHttpRequestHelper,
    IBotCommandFactory botCommandFactory,
    IMediator mediator,
    IBotCommandRepository botCommandRepository,
    ILogger<DiscordInteractionService> logger)
{
    public IDiscordHttpRequestHelper DiscordHttpRequestHelper { get; set; } = discordHttpRequestHelper;
    public IBotCommandFactory BotCommandFactory { get; set; } = botCommandFactory;
    public IMediator Mediator { get; set; } = mediator;
    public IBotCommandRepository BotCommandRepository { get; set; } = botCommandRepository;
    public ILogger<DiscordInteractionService> Logger { get; set; } = logger;
}