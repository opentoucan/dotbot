using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json.Serialization;
using Xkcd.API.Extensions;
using Xkcd.API.Models;
using XkcdApi;
using static FluentResults.Result;

namespace Xkcd.API.Services;

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
        var comic = await _httpClient.GetFromJsonAsync<XkcdComic>(url);
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