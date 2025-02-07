namespace Dotbot.Api.Dto;

public class XkcdComic
{
    public int ComicNumber { get; set; }
    public DateTime DatePosted { get; set; }
    public string AltText { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Title { get; set; } = null!;
}