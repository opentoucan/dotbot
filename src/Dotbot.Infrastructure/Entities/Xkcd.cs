namespace Dotbot.Infrastructure.Entities;

public sealed class Xkcd : Entity
{
    public int ComicNumber { get; set; }
    public DateTimeOffset Posted { get; set; }

    public Xkcd(int comicNumber, DateTimeOffset posted)
    {
        ComicNumber = comicNumber;
        Posted = posted;
    }

}