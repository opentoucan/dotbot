using Discord.Application.BotCommands;
using Discord.Application.DomainEvents.Events;
using Discord.Application.Entities;
using Discord.Application.Models;
using Discord.Application.Repositories;
using Discord.Discord;
using Discord.WebSocket;
using MediatR;

namespace Discord.Application.DomainEvents.EventHandlers;

public class BotCommandButtonReceivedHandler : INotificationHandler<ButtonActionReceivedNotification>
{
    private readonly IDiscordServerRepository _discordServerRepository;
    private readonly IMediator _mediator;
    
    public BotCommandButtonReceivedHandler(IDiscordServerRepository discordServerRepository, IMediator mediator)
    {
        _discordServerRepository = discordServerRepository;
        _mediator = mediator;
    }

    public async Task Handle(ButtonActionReceivedNotification notification, CancellationToken cancellationToken)
    {
        var user = notification.SocketMessageComponent.User as SocketGuildUser;
        var userRole =
            user.Roles.Any(x => x.Permissions.ManageChannels)
                ? Privilege.Moderator
                : Privilege.Base;

        var server = await GetServer(notification);
        var serviceContext = new DiscordChannelMessageContext(notification.SocketMessageComponent.Message, server);
            
        var defaultBotCommand = new DefaultBotCommand(userRole, serviceContext, notification.SocketMessageComponent.Data.CustomId, notification.SocketMessageComponent);
        
        await _mediator.Send(defaultBotCommand, cancellationToken);
        await notification.SocketMessageComponent.Message.DeleteAsync();

    }
    
    private async Task<DiscordServer?> GetServer(ButtonActionReceivedNotification notification)
    {
        DiscordServer? server = null;
        
        if (notification.SocketMessageComponent.Message.Channel is not SocketGuildChannel sgc) return server;
        
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