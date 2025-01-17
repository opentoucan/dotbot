using System.Text.Json;
using System.Text.Json.Serialization;
using Dotbot.Gateway.Dto;

namespace Dotbot.Gateway.Services;

public interface IXkcdService
{
    Task<XkcdComic?> GetXkcdComicAsync(int? comicNumber = null, CancellationToken cancellationToken = default);
}

public class XkcdService : IXkcdService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XkcdService> _logger;

    public XkcdService(HttpClient httpClient, ILogger<XkcdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<XkcdComic?> GetXkcdComicAsync(int? comicNumber = null, CancellationToken cancellationToken = default)
    {
        var url = comicNumber is null ? "info.0.json" : $"{comicNumber}/info.0.json";
        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch XKCD from endpoint: {url} with error: {exception}", url, ex);
            return null;
        }

        return null;
    }

    public class XkcdContent
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