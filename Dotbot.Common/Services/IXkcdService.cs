using FluentResults;

namespace Dotbot.Common.Services;

public interface IXkcdService
{
    Task<Result<XkcdComic>> GetLatestComic();
    Task<Result<XkcdComic>> GetComic(int? number = null);
    Task<Result<int>> GetLast();
    Task SetLast(int last);
}