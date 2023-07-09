namespace Discord.Application.IntegrationEvents.Events;

public class XkcdPostedEvent
{
    public XkcdPostedEvent(int comicNumber, DateTimeOffset datePosted, string altText, string imageUrl, string title)
    {
        ComicNumber = comicNumber;
        DatePosted = datePosted;
        AltText = altText;
        ImageUrl = imageUrl;
        Title = title;
    }
    public int ComicNumber { get; set; }
    public DateTimeOffset DatePosted { get; set; }
    public string AltText { get; set; }
    public string ImageUrl { get; set; }
    public string Title { get; set; }
}