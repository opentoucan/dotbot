using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities.Reports;

namespace Dotbot.Api.Discord;

public interface IHttpInteractionCommandLogger
{
    Task LogCommand(string commandName, string identifier, string guildId, string userId,
        CancellationToken cancellationToken);
}

public class HttpInteractionCommandLogger(DotbotContext dbContext, ILogger<HttpInteractionCommandLogger> logger)
    : IHttpInteractionCommandLogger
{
    public async Task LogCommand(string commandName, string identifier, string guildId, string userId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Saving audit trail for command {commandName} with identifier {identifier}", commandName,
            identifier);
        await dbContext.DiscordCommandLogs.AddAsync(new DiscordCommandLog
        {
            CommandName = commandName,
            Identifier = identifier,
            Timestamp = DateTimeOffset.UtcNow,
            GuildId = guildId,
            UserId = userId
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}