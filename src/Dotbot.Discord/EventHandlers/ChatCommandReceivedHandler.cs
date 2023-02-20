using Discord.WebSocket;
using Dotbot.Common.CommandHandlers;
using Dotbot.Common.Factories;
using Dotbot.Common.Models;
using Dotbot.Common.Services;
using Dotbot.Common.Settings;
using Dotbot.Database.Entities;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Dotbot.Discord.EventHandlers;

public class ChatCommandReceivedHandler : INotificationHandler<DiscordMessageReceivedNotification>
{
    private readonly ILogger _logger;
    private readonly IBotCommandHandlerFactory _commandHandlerFactory;
    private readonly IChatServerService _chatServerService;
    private readonly BotSettings _botSettings;
    private readonly Tracer _tracer;

    public ChatCommandReceivedHandler(IBotCommandHandlerFactory commandHandlerFactory,
        ILogger<ChatCommandReceivedHandler> logger, IOptions<BotSettings> botSettings,
        IChatServerService chatServerService, Tracer tracer)
    {
        _commandHandlerFactory = commandHandlerFactory;
        _logger = logger;
        _chatServerService = chatServerService;
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

    private async Task<ChatServer?> GetServer(DiscordMessageReceivedNotification notification)
    {
        ChatServer? server = null;
        
        if (notification.Message.Channel is not SocketGuildChannel sgc) return server;
        
        var getServerResult = await _chatServerService.Get(sgc.Guild.Id.ToString());

        if (getServerResult.IsFailed)
        {
            var createServerResult = await _chatServerService.Create(sgc.Guild.Id.ToString());
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