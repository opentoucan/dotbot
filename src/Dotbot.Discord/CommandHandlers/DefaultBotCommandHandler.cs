using System.Text.Json;
using Dotbot.Discord.Entities;
using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class DefaultBotCommandHandler : BotCommandHandler
{
    private readonly IFileService _fileService;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public DefaultBotCommandHandler(IFileService fileService, IHttpClientFactory httpClientFactory)
    {
        _fileService = fileService;
        _httpClientFactory = httpClientFactory;
    }

    public override CommandType CommandType => CommandType.Default;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        
        var messageSplit = content.Split(' ');

        var key = messageSplit[0];
        var command = await httpClient.GetFromJsonAsync<BotCommand>($"{await context.GetServerId()}?name={key}",
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (command != null)
        {
            if (Equals(command.Type, BotCommandType.String))
            {
                await HandleString(command, context);
                return Ok();
            }

            if (Equals(command.Type, BotCommandType.File))
            {
                await HandleFile(command, context);
                return Ok();
            }
        }

        await context.SendMessageAsync($"No command {key} found");

        return Fail($"No command {key} found");
    }

    private async Task<Result> HandleFile(BotCommand command, IServiceContext context)
    {
        if (!Equals(command.Type, BotCommandType.File)) return Fail("Command is not a file");
        var fileStream = await _fileService.GetFile($"{command.Content}");
        if (fileStream.IsFailed)
        {
            await context.SendMessageAsync($"Cannot find file content for {command.Name}");
            return Fail($"Cannot find file content for {command.Name}");
        }

        await context.SendFileAsync(command.Content, fileStream.Value);
        return Ok();
    }

    private static async Task<Result> HandleString(BotCommand command, IServiceContext context)
    {
        await context.SendMessageAsync(command.Content);
        return Ok();
    }
    
}