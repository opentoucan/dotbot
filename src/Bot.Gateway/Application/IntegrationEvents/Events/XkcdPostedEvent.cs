namespace Bot.Gateway.Application.IntegrationEvents.Events;

public class XkcdPostedEvent
{
    public XkcdPostedEvent(int comicNumber, DateTime datePosted, string altText, string imageUrl, string title)
    {
        ComicNumber = comicNumber;
        DatePosted = datePosted;
        AltText = altText;
        ImageUrl = imageUrl;
        Title = title;
    }
    public int ComicNumber { get; set; }
    public DateTime DatePosted { get; set; }
    public string AltText { get; set; }
    public string ImageUrl { get; set; }
    public string Title { get; set; }
}