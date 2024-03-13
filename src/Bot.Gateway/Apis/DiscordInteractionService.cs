using System.Diagnostics;
using System.Diagnostics.Metrics;
using Bot.Gateway.Application.InteractionCommands;
using Bot.Gateway.Infrastructure;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Services;
using MassTransit;

namespace Bot.Gateway.Apis;

public class DiscordInteractionService(
    IBus bus,
    IInteractionCommandFactory interactionCommandFactory,
    IBotCommandRepository botCommandRepository,
    ILogger<DiscordInteractionService> logger,
    IMeterFactory meterFactory,
    DbContext dbContext)
{
    public IBus Bus { get; set; } = bus;
    public IInteractionCommandFactory InteractionCommandFactory { get; set; } = interactionCommandFactory;
    public IBotCommandRepository BotCommandRepository { get; set; } = botCommandRepository;
    public ILogger<DiscordInteractionService> Logger { get; set; } = logger;
    public IMeterFactory MeterFactory { get; } = meterFactory;
    public DbContext DbContext { get; } = dbContext;
    public static ActivitySource ActivitySource { get; } = new(nameof(DiscordInteractionApi));
}