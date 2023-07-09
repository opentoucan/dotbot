using System.Text.Json;
using Discord.Application.Models;
using MediatR;
using StrawberryShake;

namespace Discord.Application.BotCommands;

public class SavedCommandHandler : IRequestHandler<SavedCommand, bool>
{
    private const int MaxPageSize = 20;
    private readonly IHttpClientFactory _httpClientFactory;
    public SavedCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<bool> Handle(SavedCommand request, CancellationToken cancellationToken)
    {
        var split = request.Content.Split(' ');
        var page = 1;

        if (split.Length > 1 && !int.TryParse(split[1], out page)) page = 1;

        var serverId = await request.ServiceContext.GetServerId();

        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");


        try
        {
            var result =
                await httpClient.GetFromJsonAsync<PaginatedItemsViewModel<Models.BotCommand>>(
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

            await request.ServiceContext.SendFormattedMessageAsync(formattedMessage);
        }
        catch (Exception)
        {
            var error = page < 0 ? "Invalid page" : "No commands found";
            await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage.Error(error));
            return false;
        }


        return true;
    }
}