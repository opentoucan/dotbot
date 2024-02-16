using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bot.Gateway.Application.InteractionCommands;
using Bot.Gateway.Model.Requests.Discord;
using Bot.Gateway.Model.Responses.Discord;
using Discord;
using Microsoft.AspNetCore.Mvc;
using InteractionResponseType = Bot.Gateway.Model.Responses.Discord.InteractionResponseType;

namespace Bot.Gateway.Apis;

public static class DiscordInteractionApi
{
    public static RouteGroupBuilder MapDiscordInteractionApi(this RouteGroupBuilder app)
    {
        app
            .MapPost("/", Interaction);
        return app;
    }
    
    public async static Task<IResult> Interaction([AsParameters] DiscordInteractionService service, [FromBody]InteractionRequest request, CancellationToken token)
    {
        var serializerSettings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        //Discord health check
        if (request.Type == (int)InteractionType.Ping)
        {
            return Results.Json(new InteractionResponse { Type = InteractionResponseType.Ping }, serializerSettings);
        }
        
        //Check if there is an interaction command that matches the endpoint
        var canParse = Enum.TryParse<BotCommandType>(request.Data?.Name!, true, out var botCommandType);
        if(!canParse)
            return TypedResults.NotFound($"Command not found for {request.Data?.Name!}");
        
        //If the interaction is an autocomplete request then handle it here
        if (request.Type == (int)InteractionType.ApplicationCommandAutocomplete && botCommandType == BotCommandType.Custom)
        {
            //This needs refactoring into some kind of polymorphic handler
            var commandJson = (JsonElement?)request.Data!.Options!.FirstOrDefault()?.Value;
            var botCommands = service.BotCommandRepository.SearchByNameAndServer(request.Data!.GuildId!, commandJson?.GetString(), 10);
            
            //The choices should use an automapper
            return Results.Json(new InteractionResponse
            {
                Type = InteractionResponseType.AutocompleteResponse,
                Data = new InteractionData(choices: botCommands.Select(bc => new Choice { Name = bc.Name, Value = bc.Name }).ToList())
            }, serializerSettings);
        }
        
        //Otherwise create the command, run it on another thread and continue with a followup response back to the discord API
        var command = service.BotCommandFactory.Create(botCommandType, request);
        await Task.Run( () => service.Mediator.Send(command, token), token)
            .ContinueWith(async data =>
            {
                await service.DiscordHttpRequestHelper.SendFollowupMessageAsync(ulong.Parse(request.ApplicationId!), request.Token!, await data, token);
            }, token);

        return Results.Json(new InteractionResponse{ Type = InteractionResponseType.DeferredInteractionResponse}, serializerSettings);
    }
}

