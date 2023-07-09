using MediatR;

namespace Discord.Application.BotCommands;

public class PingBotCommandHandler: IRequestHandler<PingBotCommand, bool>
{
    public async Task<bool> Handle(PingBotCommand request, CancellationToken cancellationToken)
    {
        await request.ServiceContext.ReplyAsync("pong");
        return true;
    }
}