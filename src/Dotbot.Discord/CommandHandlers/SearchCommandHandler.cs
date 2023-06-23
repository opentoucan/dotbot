using System.Drawing;
using System.Text.Json;
using Dotbot.Discord.Entities;
using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class SearchCommandHandler : BotCommandHandler
{
    public override CommandType CommandType => CommandType.Search;
    public override Privilege PrivilegeLevel => Privilege.Base;
    private readonly IHttpClientFactory _httpClientFactory;

    public SearchCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        var split = content.Split(' ');

        if (split.Length <= 1)
        {
            return Fail("No search term given");
        }

        var serverId = await context.GetServerId();
        var searchTerm = split[1];
        var result = await httpClient.GetFromJsonAsync<List<FuzzySearchViewModel>>($"search/{serverId}?searchTerm={searchTerm}&limit={20}&cutoff={20}",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (result == null)
        {
            return Fail($"Failed to retrieve bot command {split[1]}");
        }
        
        var fm = FormattedMessage
            .Info($"Command - Closeness to {split[1]}")
            .SetTitle("Matched commands")
            .AppendDescription("")
            .SetColor(Color.FromArgb(157, 3, 252));

        foreach (var fuzzySearchResult in result.OrderByDescending(x => x.Score))
        {
            fm.AppendDescription($"{fuzzySearchResult.Value} - {fuzzySearchResult.Score}%");
        }

        await context.SendFormattedMessageAsync(fm);
        return Ok();
    }
}