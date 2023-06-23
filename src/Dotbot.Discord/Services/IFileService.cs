using FluentResults;

namespace Dotbot.Discord.Services;

public interface IFileService
{
    Task<Result<Stream>> GetFile(string fileName);
    Task<Result> SaveFile(string fileName, Stream fileStream);
}
