namespace Xkcd.API.Dtos;

public class XkcdComic
{
    public int Month { get; set; }
    public int Num { get; set; }
    public int Year { get; set; }
    public required string Alt { get; set; }
    public required string Img { get; set; }
    public required string Title { get; set; }
    public int Day { get; set; }
}