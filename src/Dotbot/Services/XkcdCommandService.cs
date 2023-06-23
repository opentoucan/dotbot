using Dotbot.Models;

namespace Dotbot.Services;

public class XkcdCommandService : IXkcdCommandService
{
   private readonly XkcdService.XkcdServiceClient _client;

   public XkcdCommandService(XkcdService.XkcdServiceClient client)
   {
      _client = client;
   }

   public XkcdComic GetXkcd(int? comicNumber = null)
   {
      var xkcdRequest = new XkcdRequest();
      if (comicNumber != null)
         xkcdRequest.Id = comicNumber.Value;
      return MapFromXkcdResponse(_client.GetXkcd(xkcdRequest));
   }

   private XkcdComic MapFromXkcdResponse(XkcdResponse xkcdResponse)
   {
      return new XkcdComic
      {
         ComicNumber = xkcdResponse.Id,
         AltText = xkcdResponse.AltText,
         DatePosted = xkcdResponse.PublishedDate.ToDateTimeOffset(),
         ImageUrl = xkcdResponse.ImageUrl,
         Title = xkcdResponse.Title
      };
   }
}