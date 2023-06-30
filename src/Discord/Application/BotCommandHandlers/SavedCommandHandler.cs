using System.Text.Json;
using Discord.Application.Models;
using FluentResults;
using static FluentResults.Result;

namespace Discord.BotCommandHandlers;

public class SavedCommandHandler : BotCommandHandler
{
    private const int MaxPageSize = 20;
    private readonly IHttpClientFactory _httpClientFactory;

    public SavedCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override CommandType CommandType => CommandType.Saved;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var split = content.Split(' ');
        var page = 1;

        if (split.Length > 1 && !int.TryParse(split[1], out page)) page = 1;

        var serverId = await context.GetServerId();

        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");


        try
        {
            var result =
                await httpClient.GetFromJsonAsync<PaginatedItemsViewModel<BotCommand>>(
                    $"commands/{serverId}?pageSize={MaxPageSize}&pageIndex={page - 1}",
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var totalPages = Math.Ceiling((decimal)result?.Data.Count() / MaxPageSize);

            var formattedMessage = FormattedMessage
                .Info()
                .SetTitle("Saved Commands")
                .SetDescription($"Page {page} of {totalPages} pages ({result.Data.Count()} saved commands)")
                .SetColor(System.Drawing.Color.FromArgb(157, 3, 252));

            foreach (var command in result.Data)
            {
                formattedMessage.AddField(command.Name,command.Content[..Math.Min(command.Content.Length, 40)], true);
            }

            await context.SendFormattedMessageAsync(formattedMessage);
        }
        catch (Exception)
        {
            var error = page < 0 ? "Invalid page" : "No commands found";
            await context.SendFormattedMessageAsync(FormattedMessage.Error(error));
            return Fail(error);
        }


        return Ok();
    }
}