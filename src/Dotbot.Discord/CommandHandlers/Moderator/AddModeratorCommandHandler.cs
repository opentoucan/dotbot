using Dotbot.Discord.Services;
using FluentResults;
using static Dotbot.Discord.Models.FormattedMessage;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers.Moderator;

public class AddModeratorCommandHandler: BotCommandHandler
{
    public AddModeratorCommandHandler(IChatServerService chatServerService)
    {
        _chatServerService = chatServerService;
    }

    public override CommandType CommandType => CommandType.AddModerator;
    public override Privilege PrivilegeLevel => Privilege.Moderator;

    private readonly IChatServerService _chatServerService;
    
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
            var result = await _chatServerService.AddModerator(serverId, mention.Id.ToString());
            if(result.IsFailed)
            {
                await context.SendFormattedMessageAsync(Error(result.Errors));
                return Fail(result.Reasons.ToString()); 
            }
        }

        await context.SendFormattedMessageAsync(Success("Users added as moderators"));
        
        return Ok();
    }
    
}