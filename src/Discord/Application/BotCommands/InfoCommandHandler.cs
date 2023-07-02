﻿using System.Reflection;
using System.Text.Json;
using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;
using FluentResults;
using MediatR;
using static FluentResults.Result;

namespace Discord.Application.BotCommands;

public class InfoCommandHandler: IRequestHandler<InfoCommand, bool>
{
    
    private readonly IHttpClientFactory _httpClientFactory;

    public InfoCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private async Task SendInfoMessage(IServiceContext context)
    {
        await context.SendFormattedMessageAsync(FormattedMessage
            .Info()
            .SetTitle("Dotbot.API")
            .SetDescription("This is a Discord bot written in C# & .NET 7")
            .AddField("Version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Version Not Set")
            .AddField("Developers", "Daniel Harman\nKieran Dennis\nJared Prest")
            .AddField("Libraries", "https://github.com/discord-net/Discord.Net, https://github.com/altmann/FluentResults, https://github.com/Tyrrrz/YoutubeExplode")
            .AddField("Source", "https://github.com/Sillock-Inc/Dobot")
            .AddField("Licence", "https://www.apache.org/licenses/LICENSE-2.0")
        );
    }

    public async Task<bool> Handle(InfoCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        var split = request.Content.Split(' ');

        if (split.Length <= 1)
        {
            await SendInfoMessage(request.ServiceContext);
        }
        else
        {
            var command = await httpClient.GetFromJsonAsync<Models.BotCommand>($"{request.ServiceContext.GetServerId()}?name={split[1]}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (command != null)
            {
                var creatorName = "Unknown";
                if (command.CreatorId != null)
                {
                    var user = await request.ServiceContext.GetUserAsync(ulong.Parse(command.CreatorId));
                    if (user != null)
                    {
                        creatorName = user.Nickname;
                    }
                }

                var fm = FormattedMessage
                    .Info()
                    .SetTitle($"Command: {command.Name}")
                    .SetDescription(command.Content)
                    .AddField("Creator", creatorName)
                    .AddField("Created", command.Created.ToString("f"));
                await request.ServiceContext.SendFormattedMessageAsync(fm);
            }
            else
            {
                await SendInfoMessage(request.ServiceContext);
            }
        }

        return true;
    }
}