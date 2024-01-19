using Bot.Gateway.Model.Requests.Discord;
using Bot.Gateway.Model.Responses.Discord;
using Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class AvatarCommandHandler : IRequestHandler<AvatarCommand, InteractionData>
{
    public Task<InteractionData> Handle(AvatarCommand request, CancellationToken cancellationToken)
    {
        var mentionedUser = request.Data.Data!.Resolved!.Users!.FirstOrDefault().Value;
        var userId = ulong.Parse(mentionedUser.Id!);
        var avatarId = mentionedUser.Avatar!;
        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithImageUrl($"https://cdn.discordapp.com/avatars/{userId}/{avatarId}?size=512");
        embedBuilder.WithTitle(request.Data.Data.Resolved.Users!.FirstOrDefault().Value.Username);
        embedBuilder.WithDescription("Avatar");
        var interactionData = new InteractionData(embeds: [embedBuilder.Build()]);
        return Task.FromResult(interactionData);
    }
}

public class AvatarCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Avatar;
    public override InteractionRequest Data { get; set; } = null!;
}