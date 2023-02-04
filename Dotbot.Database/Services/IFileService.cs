using FluentResults;

namespace Dotbot.Database.Services;

public interface IFileService
{
    Task<Result<Stream>> GetFile(string fileName);
    Task<Result> SaveFile(string fileName, Stream fileStream);
}
