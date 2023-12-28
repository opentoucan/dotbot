namespace Bot.Gateway.Model.Responses;

public class XkcdComic
{
    public int ComicNumber { get; set; }
    public DateTimeOffset DatePosted { get; set; }
    public required string AltText { get; set; }
    public required string ImageUrl { get; set; }
    public required string Title { get; set; }
}