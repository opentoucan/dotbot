using Bot.Gateway.Model.Responses;

namespace Bot.Gateway.Services;

public interface IXkcdCommandService
{
    XkcdComic GetXkcd(int? comicNumber = null);
}