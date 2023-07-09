using System.Text.Json;
using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Application.Services;
using Discord.Discord;
using Discord.WebSocket;
using FluentResults;
using MediatR;
using static FluentResults.Result;

namespace Discord.Application.BotCommands;

public class DefaultBotCommandHandler : IRequestHandler<DefaultBotCommand, bool>
{
    private readonly IFileService _fileService;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public DefaultBotCommandHandler(IFileService fileService, IHttpClientFactory httpClientFactory)
    {
        _fileService = fileService;
        _httpClientFactory = httpClientFactory;
    }
    

    private async Task<Result> HandleFile(Models.BotCommand command, IServiceContext context, SocketMessageComponent? socketMessageComponent = null)
    {
        if (!Equals(command.Type, BotCommandType.File)) return Fail("Command is not a file");
        var fileStream = await _fileService.GetFile($"{command.Content}");
        if (fileStream.IsFailed)
        {
            await context.SendMessageAsync($"Cannot find file content for {command.Name}");
            return Fail($"Cannot find file content for {command.Name}");
        }

        if (socketMessageComponent == null)
            await context.SendFileAsync(command.Content, fileStream.Value);
        else
            await socketMessageComponent.RespondWithFileAsync(fileStream.Value,command.Content);
        return Ok();
    }

    private static async Task<Result> HandleString(Models.BotCommand command, IServiceContext context, SocketMessageComponent? socketMessageComponent = null)
    {
        if (socketMessageComponent == null)
            await context.SendMessageAsync(command.Content);
        else
            await socketMessageComponent.RespondAsync(command.Content);
        return Ok();
    }

    public async Task<bool> Handle(DefaultBotCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        
        var messageSplit = request.Content.Split(' ');

        var key = messageSplit[0];

        try
        {
            var command = await httpClient.GetFromJsonAsync<Models.BotCommand>(
                $"{await request.ServiceContext.GetServerId()}?name={key}",
                new JsonSerializerOptions
                    { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (command != null)
            {
                if (Equals(command.Type, BotCommandType.String))
                {
                    await HandleString(command, request.ServiceContext, request.SocketMessageComponent);
                    return false;
                }

                if (Equals(command.Type, BotCommandType.File))
                {
                    await HandleFile(command, request.ServiceContext, request.SocketMessageComponent);
                    return false;
                }
            }
        }
        catch
        {
            await request.ServiceContext.SendMessageAsync($"No command {key} found");
        }

        return false;
    }
}