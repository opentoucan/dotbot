using Dotbot.Models;

namespace Dotbot.Services;

public interface IXkcdCommandService
{
    XkcdComic GetXkcd(int? comicNumber = null);
}