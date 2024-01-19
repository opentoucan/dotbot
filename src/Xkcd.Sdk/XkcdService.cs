using System.Text.Json;
using System.Text.Json.Serialization;
using Xkcd.Sdk;

namespace Xkcd.Sdk;

public class XkcdService
{
    private readonly HttpClient _httpClient;

    public XkcdService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<XkcdComic?> GetXkcdComicAsync(int? comicNumber, CancellationToken cancellationToken)
    {
        var url = comicNumber is null ? "info.0.json" : $"{comicNumber}/info.0.json";
        
        var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
        if (httpResponse.IsSuccessStatusCode)
        {
            var xkcdContent = await JsonSerializer.DeserializeAsync<XkcdContent>(
                await httpResponse.Content.ReadAsStreamAsync(cancellationToken), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }, cancellationToken);
            return MapFromXkcdContent(xkcdContent!);
        }
        return null;
    }
    
    private class XkcdContent
    {
        public int Month { get; set; }
        public int Num { get; set; }
        public int Year { get; set; }
        public required string Alt { get; set; }
        public required string Img { get; set; }
        public required string Title { get; set; }
        public int Day { get; set; }
    }
    
    private XkcdComic MapFromXkcdContent(XkcdContent xkcdResponse)
    {
        return new XkcdComic
        {
            ComicNumber = xkcdResponse!.Num,
            AltText = xkcdResponse.Alt,
            ImageUrl = xkcdResponse.Img,
            Title = xkcdResponse.Title,
            DatePosted = new DateTime(xkcdResponse.Year, xkcdResponse.Month, xkcdResponse.Day)
        };
    }
}