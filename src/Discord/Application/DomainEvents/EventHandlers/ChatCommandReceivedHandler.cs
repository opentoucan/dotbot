using Discord.Application.BotCommandHandlers;
using Discord.Application.BotCommands;
using Discord.Application.DomainEvents.Events;
using Discord.Application.Entities;
using Discord.Application.Factories;
using Discord.Application.Models;
using Discord.Application.Repositories;
using Discord.Discord;
using Discord.Exceptions;
using Discord.Extensions;
using Discord.Settings;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Discord.Application.DomainEvents.EventHandlers;

public class ChatCommandReceivedHandler : INotificationHandler<DiscordMessageReceivedNotification>
{
    private readonly ILogger _logger;
    private readonly IDiscordServerRepository _discordServerRepository;
    private readonly BotSettings _botSettings;
    private readonly Tracer _tracer;
    private readonly IMediator _mediator;
    private readonly IBotCommandCreatorFactory _botCommandCreatorFactory;
    
    public ChatCommandReceivedHandler(ILogger<ChatCommandReceivedHandler> logger, IOptions<BotSettings> botSettings,
        IDiscordServerRepository discordServerRepository, Tracer tracer, IMediator mediator, IBotCommandCreatorFactory botCommandCreatorFactory)
    {
        _logger = logger;
        _discordServerRepository = discordServerRepository;
        _tracer = tracer;
        _mediator = mediator;
        _botCommandCreatorFactory = botCommandCreatorFactory;
        _botSettings = botSettings.Value;
    }

    public async Task Handle(DiscordMessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("<{AuthorUsername}>: {Message}", notification.Message.Author.Username,
            notification.Message.Content);

        var server = await GetServer(notification);

        var messageSplit = notification.Message.Content.Split(' ');

        var channelGuild = notification.Message.Channel.AsGuildChannel();

        var user = notification.Message.Author as SocketGuildUser;

        if (messageSplit[0].StartsWith(_botSettings.CommandPrefix))
        {
            var userRole =
                channelGuild != null && user.Roles.Any(x => x.Permissions.ManageChannels)
                    ? Privilege.Moderator
                    : Privilege.Base;

            var commandName = messageSplit[0][1..];
            Enum.TryParse<BotCommandTypes>(commandName, true, out var commandType);
            var botCommand = _botCommandCreatorFactory.Create(commandType, userRole,
                new DiscordChannelMessageContext(notification.Message, server), notification.Message.Content[1..]);

            try
            {
                await _mediator.Send(botCommand, cancellationToken);
            }
            catch (BotCommandPermissionException ex)
            {
                _logger.LogWarning("<{AuthorUsername}> attempted to use a command with elevated privileges: {CommandName}", notification.Message.Author.Username,
                    commandName);
                await notification.Message.Channel.SendMessageAsync("You do not have permission to use this command");
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