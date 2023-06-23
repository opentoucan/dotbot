using System.Text.Json.Serialization;

namespace Xkcd.API.Dtos;

public class XkcdComic
{
    public int Month { get; set; }
    public int Num { get; set; }
    public int Year { get; set; }
    public string Alt { get; set; }
    public string Img { get; set; }
    public string Title { get; set; }
    public int Day { get; set; }
}