using System.Reflection;
using Dotbot.Common.Models;
using Dotbot.Database.Repositories;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.CommandHandlers;

public class InfoCommandHandler: BotCommandHandler
{
    public override CommandType CommandType => CommandType.Info;
    public override Privilege PrivilegeLevel => Privilege.Base;
    
    private readonly IBotCommandRepository _botCommandRepository;
    
    public InfoCommandHandler(IBotCommandRepository botCommandRepository)
    {
        _botCommandRepository = botCommandRepository;
    }
    
    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var split = content.Split(' ');

        if (split.Length <= 1)
        {
            await SendInfoMessage(context);
        }
        else
        {
            var (isSuccess, _, command) = await _botCommandRepository.GetCommand(await context.GetServerId(), split[1]);
            if (isSuccess)
            {
                var creatorName = "Unknown";
                if (command.CreatorId != null)
                {
                    var user = await context.GetUserAsync(ulong.Parse(command.CreatorId));
                    if (user != null)
                    {
                        creatorName = user.Nickname;
                    }
                }

                var fm = FormattedMessage
                    .Info()
                    .SetTitle(command.Key)
                    .SetDescription(command.Content ?? command.FileName ?? "No Content")
                    .AddField("Creator", creatorName)
                    .AddField("Created", command.Created.ToString("O"));
                await context.SendFormattedMessageAsync(fm);
            }
            else
            {
                await SendInfoMessage(context);
            }
        }

        return Ok();
    }

    private async Task SendInfoMessage(IServiceContext context)
    {
        await context.SendFormattedMessageAsync(FormattedMessage
            .Info()
            .SetTitle("Dotbot")
            .SetDescription("This is a Discord bot written in C# & .NET 7")
            .AddField("Version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Version Not Set")
            .AddField("Developers", "Daniel Harman\nKieran Dennis\nJared Prest")
            .AddField("Libraries", "https://github.com/discord-net/Discord.Net, https://github.com/altmann/FluentResults, https://github.com/Tyrrrz/YoutubeExplode")
            .AddField("Source", "https://github.com/Sillock-Inc/Dobot")
            .AddField("Licence", "https://www.apache.org/licenses/LICENSE-2.0")
        );
    }
    
}