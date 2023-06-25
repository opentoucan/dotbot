using System.Runtime.Serialization;
using MediatR;

namespace Xkcd.API.Commands;

public class SetXkcdCommand : IRequest<bool>
{
    [DataMember]
    public int ComicNumber { get; set; }
    
    [DataMember]
    public string ImageUrl { get; set; }
    
    [DataMember]
    public string Title { get; set; }
    
    [DataMember]
    public string AltText { get; set; }
    
    [DataMember]
    public DateTimeOffset DatePosted { get; set; }
    
    public SetXkcdCommand(int comicNumber, string imageUrl, string title, string altText, DateTimeOffset datePosted)
    {
        ComicNumber = comicNumber;
        ImageUrl = imageUrl;
        Title = title;
        AltText = altText;
        DatePosted = datePosted;
    }

    public SetXkcdCommand()
    {
        
    }
}