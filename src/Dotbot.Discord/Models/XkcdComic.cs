using System.Text.Json.Serialization;

namespace Dotbot.Discord.Models;

public class XkcdComic
{
    public int ComicNumber { get; set; }
    public DateTimeOffset DatePosted { get; set; }
    public string AltText { get; set; }
    public string ImageUrl { get; set; }
    public string Title { get; set; }
}