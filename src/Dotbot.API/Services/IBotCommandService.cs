using FluentResults;

namespace Dotbot.Services;

public interface IBotCommandService
{
    Task<Result<dynamic>> FindBotCommand(string serviceId, string commandName);
}