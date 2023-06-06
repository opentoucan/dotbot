using Discord;
using Discord.WebSocket;
using MediatR;

namespace Dotbot.Discord.EventListeners;

public class ReactionEventListener
{
    private readonly IMediator _mediator;

    public ReactionEventListener(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        if(arg3.User.Value.IsBot) return;
        await arg2.Value.SendMessageAsync("FUCK");
    }

    public async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        if(arg3.User.Value.IsBot) return;
        await arg2.Value.SendMessageAsync("KCUF");
    }
}