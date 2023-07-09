using Discord.Application.Repositories;
using MediatR;
using static Discord.Application.Models.FormattedMessage;

namespace Discord.Application.BotCommands;

public class SetXkcdChannelCommandHandler : IRequestHandler<SetXkcdChannelCommand, bool>
{
    private readonly IDiscordServerRepository _discordServerRepository;

    public SetXkcdChannelCommandHandler(IDiscordServerRepository discordServerRepository)
    {
        _discordServerRepository = discordServerRepository;
    }

    public async Task<bool> Handle(SetXkcdChannelCommand request, CancellationToken cancellationToken)
    {
        var channelId  = await request.ServiceContext.GetChannelId();
        var serverId = await request.ServiceContext.GetServerId();
        try
        {
            await _discordServerRepository.SetXkcdChannelId(serverId, channelId);
        }
        catch (Exception)
        {
            var errorMessage = $"Failed to set XKCD channel";
            await request.ServiceContext.SendFormattedMessageAsync(Error(errorMessage));
            return false;
        }
        
        await request.ServiceContext.SendFormattedMessageAsync(Success("Channel set as XKCD channel"));
        return true;
    }
}