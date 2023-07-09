using Discord.Application.Models;
using MediatR;

namespace Discord.Application.BotCommands;

public class AvatarCommandHandler : IRequestHandler<AvatarCommand, bool>
{
    public async Task<bool> Handle(AvatarCommand request, CancellationToken cancellationToken)
    {
        var mentionedIds = await request.ServiceContext.GetUserMentionsAsync();

        foreach (var user in mentionedIds)
        {
            await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage
                .Info()
                .SetTitle(user.Username)
                .SetImage(user.EffectiveAvatarUrl));
        }

        return true;
    }
}