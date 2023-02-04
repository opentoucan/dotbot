using FluentResults;

namespace Dotbot.Common.Services;

public interface IXkcdSenderService
{
    Task<Result> SendNewComic(XkcdComic comic);
}