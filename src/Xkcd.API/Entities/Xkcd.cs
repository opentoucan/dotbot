namespace Xkcd.API.Entities;

public sealed class Xkcd : Entity
{
    public int ComicNumber { get; set; }
    public DateTimeOffset Posted { get; set; }

    public Xkcd(int comicNumber, DateTimeOffset posted)
    {
        Id = Guid.NewGuid();
        ComicNumber = comicNumber;
        Posted = posted;
    }
    
}