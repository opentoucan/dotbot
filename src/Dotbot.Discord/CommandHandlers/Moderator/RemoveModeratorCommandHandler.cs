using Dotbot.Discord.Repositories;
using Dotbot.Discord.Services;
using FluentResults;
using static Dotbot.Discord.Models.FormattedMessage;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers.Moderator;

public class RemoveModeratorCommandHandler : BotCommandHandler
{
    private readonly IDiscordServerRepository _discordServerRepository;

    public RemoveModeratorCommandHandler(IDiscordServerRepository discordServerRepository)
    {
        _discordServerRepository = discordServerRepository;
    }

    public override CommandType CommandType => CommandType.RemoveModerator;
    public override Privilege PrivilegeLevel => Privilege.Moderator;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var mentions = await context.GetUserMentionsAsync();

        if (mentions.Count == 0)
        {
            await context.SendFormattedMessageAsync(Error("No users provided"));
            return Fail("No users provided");
        }

        var serverId = await context.GetServerId();

        foreach (var mention in mentions)
        {
            try
            {
                await _discordServerRepository.RemoveModId(serverId, mention.Id.ToString());
            }
            catch (Exception)
            {
                var errorMessage = $"Failed to remove moderator: {mention.Username}";
                await context.SendFormattedMessageAsync(Error(errorMessage));
                return Fail(errorMessage);
            }
        }

        await context.SendFormattedMessageAsync(Success("Removed moderators"));

        return Ok();
    }
}