using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Xkcd.API.Dtos;
using XkcdApi;

namespace Xkcd.API.Grpc;

public class XkcdService : XkcdApi.XkcdService.XkcdServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XkcdService> _logger;
    public XkcdService(HttpClient httpClient, ILogger<XkcdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task<XkcdResponse> GetXkcd(XkcdRequest request, ServerCallContext context)
    {
        var url = request.Id == default ? "info.0.json" : $"{request.Id}/info.0.json";

        _logger.LogInformation($"Url: {_httpClient.BaseAddress + url}");
        _logger.LogInformation($"XkcdRequest: {request.Id}");
        
        var comic = await _httpClient.GetFromJsonAsync<XkcdComic>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, NumberHandling = JsonNumberHandling.AllowReadingFromString});
        return new XkcdResponse
        {
            Id = comic.Num,
            AltText = comic.Alt,
            ImageUrl = comic.Img,
            Title = comic.Title,
            PublishedDate = Timestamp.FromDateTimeOffset(new DateTime(comic.Year, comic.Month, comic.Day))
        };
    }
}