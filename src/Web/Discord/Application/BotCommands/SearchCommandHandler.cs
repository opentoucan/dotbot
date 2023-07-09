using System.Text.Json;
using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Commands.Builders;
using FluentResults;
using MediatR;
using StrawberryShake;
using static FluentResults.Result;

namespace Discord.Application.BotCommands;

public class SearchCommandHandler : IRequestHandler<SearchCommand, bool>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDotbotClient _dotbotGraphQLClient;
    public SearchCommandHandler(IHttpClientFactory httpClientFactory, IDotbotClient dotbotGraphQlClient)
    {
        _httpClientFactory = httpClientFactory;
        _dotbotGraphQLClient = dotbotGraphQlClient;
    }

    public async Task<bool> Handle(SearchCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        var split = request.Content.Split(' ');

        if (split.Length <= 1)
        {
            //return Fail("No search term given");
            return false;
        }
        
        var res = await _dotbotGraphQLClient.SearchBotCommand.ExecuteAsync(5, new BotCommandFilterInput{Name = new StringOperationFilterInput{Contains = split[1]}}, cancellationToken);
        if (res.IsErrorResult())
        {
            await request.ServiceContext.SendMessageAsync("Failed to search for saved commands");
            return false;
        }

        var savedCommands = res?.Data?.SearchBotCommands?.Nodes?.ToList() ?? new List<ISearchBotCommand_SearchBotCommands_Nodes>();
        
        
        var serverId = await request.ServiceContext.GetServerId();
        var searchTerm = split[1];
        //var result = await httpClient.GetFromJsonAsync<List<FuzzySearchViewModel>>($"search/{serverId}?searchTerm={searchTerm}&limit={20}&cutoff={20}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        var fm = FormattedMessage
            .Info($"Command - Closeness to {split[1]}")
            .SetTitle("Matched commands")
            .AppendDescription("")
            .SetColor(System.Drawing.Color.FromArgb(157, 3, 252));

        var buttonBuilder = new ComponentBuilder();
        
        foreach (var command in savedCommands)
        {
            buttonBuilder.WithButton($"{command.Name}", $"{command.Name}");
        }
        
        await request.ServiceContext.ReplyAsync(buttonBuilder.Build());
        return true;
    }
}