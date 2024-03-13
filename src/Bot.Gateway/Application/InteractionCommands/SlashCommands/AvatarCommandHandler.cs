using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class AvatarCommandHandler : IRequestHandler<AvatarCommand, InteractionData>
{
    public Task<InteractionData> Handle(AvatarCommand request, CancellationToken cancellationToken)
    {
        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithImageUrl($"https://cdn.discordapp.com/avatars/{request.TargetUserId}/{request.AvatarId}?size=512");
        embedBuilder.WithTitle(request.TargetUsername);
        embedBuilder.WithDescription("Avatar");
        var interactionData = new InteractionData(embeds: [embedBuilder.Build()]);
        return Task.FromResult(interactionData);
    }
}

public class AvatarCommand : InteractionCommand
{
    public override string InteractionCommandName => "avatar";
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        var mentionedUser = interactionRequest.Data!.Resolved!.Users!.FirstOrDefault().Value;
        AvatarId = mentionedUser.Avatar!;
        TargetUserId = mentionedUser.Id!;
        TargetUsername = mentionedUser.Username!;
    }

    public string TargetUserId { get; private set; } = null!;
    public string TargetUsername { get; private set; } = null!;
    public string AvatarId { get; private set; } = null!;
}