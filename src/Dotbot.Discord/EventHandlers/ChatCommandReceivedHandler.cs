using Discord.WebSocket;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.Entities;
using Dotbot.Discord.Events;
using Dotbot.Discord.Factories;
using Dotbot.Discord.Models;
using Dotbot.Discord.Repositories;
using Dotbot.Discord.Services;
using Dotbot.Discord.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Dotbot.Discord.EventHandlers;

public class ChatCommandReceivedHandler : INotificationHandler<DiscordMessageReceivedNotification>
{
    private readonly ILogger _logger;
    private readonly IBotCommandHandlerFactory _commandHandlerFactory;
    private readonly IDiscordServerRepository _discordServerRepository;
    private readonly BotSettings _botSettings;
    private readonly Tracer _tracer;

    public ChatCommandReceivedHandler(IBotCommandHandlerFactory commandHandlerFactory,
        ILogger<ChatCommandReceivedHandler> logger, IOptions<BotSettings> botSettings,
        IDiscordServerRepository discordServerRepository, Tracer tracer)
    {
        _commandHandlerFactory = commandHandlerFactory;
        _logger = logger;
        _discordServerRepository = discordServerRepository;
        _tracer = tracer;
        _botSettings = botSettings.Value;
    }

    public async Task Handle(DiscordMessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("<{AuthorUsername}>: {Message}", notification.Message.Author.Username,
            notification.Message.Content);

        var server = await GetServer(notification);

        var messageSplit = notification.Message.Content.Split(' ');

        BotCommandHandler? handler = null;
        
        if (messageSplit[0].StartsWith(_botSettings.CommandPrefix))
        {
            handler = _commandHandlerFactory.GetCommand(messageSplit[0][1..]);
        }

        if (messageSplit[0].StartsWith(_botSettings.ModCommandPrefix))
        {
            handler = _commandHandlerFactory.GetCommand(messageSplit[0][1..], Privilege.Moderator);
        }

        if (handler != null)
        {
            using var span = _tracer.StartActiveSpan("Handle-Command");
            var context = new DiscordChannelMessageContext(notification.Message, server);
            try
            {
                span.AddEvent("Start execution");
                var executionResult = await handler.HandleAsync(
                    notification.Message.Content[1..], context);
                span.AddEvent("End execution");
                
                if (executionResult.IsFailed)
                {
                    span.AddEvent("Failed to execute");
                    var errs = string.Join(", ",executionResult.Errors.Select(x => x.Message));
                    _logger.LogError("Failed to execute handler: {}", errs);
                    await context.SendFormattedMessageAsync(FormattedMessage.Error(errs));
                }
            }
            catch (Exception e)
            {
                _logger.LogError("{}",e.Message);
                await context.SendFormattedMessageAsync(FormattedMessage.Error(e.Message));
            }
            
        }
    }

    private async Task<DiscordServer?> GetServer(DiscordMessageReceivedNotification notification)
    {
        DiscordServer? server = null;
        
        if (notification.Message.Channel is not SocketGuildChannel sgc) return server;
        
        var getServerResult = await _discordServerRepository.GetServerAsync(sgc.Guild.Id.ToString());

        if (getServerResult.IsFailed)
        {
            var createServerResult = await _discordServerRepository.CreateServerAsync(sgc.Guild.Id.ToString());
            if (createServerResult.IsFailed)
            {
                throw new Exception(
                    $"Failed to create chat server on new message {string.Join(", ", createServerResult.Errors)}");
            }

            server = createServerResult.Value;
        }
        else
        {
            server = getServerResult.Value;
        }

        return server;
    }
}