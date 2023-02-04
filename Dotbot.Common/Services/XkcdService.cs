using System.Net.Http.Json;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.Services;

public class XkcdService : IXkcdService
{
    private const string LatestUrl = "https://xkcd.com/info.0.json";
    private const string ComicUrl = "https://xkcd.com/{0}/info.0.json";

    private readonly IPersistentSettingsService _persistentSettingsService;
    private readonly HttpClient _httpClient;

    public XkcdService(IPersistentSettingsService persistentSettingsService, HttpClient httpClient)
    {
        _persistentSettingsService = persistentSettingsService;
        _httpClient = httpClient;
    }
    
    public async Task<Result<XkcdComic>> GetLatestComic() => await GetComic();
    
    public async Task<Result<XkcdComic>> GetComic(int? number = null)
    {
        var url = number == null ? LatestUrl : string.Format(ComicUrl, number);
        try
        {
            var comic = await _httpClient.GetFromJsonAsync<XkcdComic>(url);
            return comic != null ? Ok(comic) : Fail("Failed to parse comic JSON");
        }
        catch (HttpRequestException ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task SetLast(int last)
    {
        await _persistentSettingsService.SetSetting("XkcdComicNumber", last);
    }
    
    public async Task<Result<int>> GetLast()
    {
        return await _persistentSettingsService.GetSetting<int>("XkcdComicNumber");
    }


    
}